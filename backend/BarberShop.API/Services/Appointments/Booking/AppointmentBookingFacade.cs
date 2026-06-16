using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Appointments;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Services.Appointments;
using BarberShop.API.Services.Notifications;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;
using AppointmentServiceEntity = BarberShop.API.Entities.AppointmentService;
using ServiceEntity = BarberShop.API.Entities.Service;

namespace BarberShop.API.Services.Appointments.Booking;

public class AppointmentBookingFacade : IAppointmentBookingFacade
{
    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public AppointmentBookingFacade(
        BarberShopDbContext dbContext,
        ICurrentUserService currentUserService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task<AppointmentDto> BookAsync(AppointmentBookRequest request)
    {
        if (request is null)
        {
            throw new BadRequestException("Appointment booking request is required.");
        }

        ValidateRequest(request);
        var serviceIds = request.ServiceIds!;
        var paymentMethod = NormalizePaymentMethod(request.PaymentMethod);

        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var client = await _dbContext.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(currentClient => currentClient.UserId == currentUserId);
        if (client is null)
        {
            throw new ForbiddenException("Current user does not have a client profile.");
        }

        var startTime = ToUtcDateTime(request.StartTime);
        ValidateStartTime(startTime);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(currentEmployee => currentEmployee.Id == request.EmployeeId);
        if (employee is null)
        {
            throw new BadRequestException("Employee does not exist.");
        }

        if (!employee.Active)
        {
            throw new BadRequestException("Employee is not active.");
        }

        var services = await _dbContext.Services
            .AsNoTracking()
            .Where(service => serviceIds.Contains(service.Id))
            .ToListAsync();

        ValidateServices(serviceIds, services, employee.ShopId);

        var totalDurationMinutes = services.Sum(service => service.DurationMinutes);
        var totalPrice = services.Sum(service => service.Price);
        var endTime = startTime.AddMinutes(totalDurationMinutes);

        ValidateCalculatedRange(startTime, endTime);
        await EnsureClientActiveFutureAppointmentLimitAsync(client.Id, employee.ShopId);
        await EnsureWithinWorkingHoursAsync(employee.Id, startTime, endTime);
        await EnsureNoTimeOffOverlapAsync(employee.Id, startTime, endTime);

        // TODO: Under high parallel booking load, add stronger database protection or an employee-level lock.
        await EnsureAppointmentSlotAvailableAsync(employee.Id, employee.ShopId, startTime, endTime, services);

        var appointment = new Appointment
        {
            ClientId = client.Id,
            EmployeeId = employee.Id,
            StartTime = startTime,
            EndTime = endTime,
            Status = AppointmentStatuses.PENDING,
            TotalPrice = totalPrice,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Appointments.Add(appointment);

        foreach (var service in OrderByRequest(serviceIds, services))
        {
            appointment.AppointmentServices.Add(new AppointmentServiceEntity
            {
                ServiceId = service.Id,
                PriceAtBooking = service.Price,
                DurationAtBooking = service.DurationMinutes
            });
        }

        _dbContext.AppointmentStatusHistories.Add(new AppointmentStatusHistory
        {
            Appointment = appointment,
            OldStatus = null,
            NewStatus = AppointmentStatuses.PENDING,
            ChangedByUserId = currentUserId,
            ChangedAt = DateTime.UtcNow
        });

        _dbContext.Payments.Add(new Payment
        {
            Appointment = appointment,
            Amount = totalPrice,
            PaymentMethod = paymentMethod,
            Status = PaymentStatuses.PENDING,
            PaidAt = null
        });

        await _dbContext.SaveChangesAsync();
        await _notificationService.CreateForAppointmentEventAsync(
            appointment.Id,
            NotificationTypes.APPOINTMENT_BOOKED,
            currentUserId);

        await transaction.CommitAsync();

        return MapToDto(appointment, services, paymentMethod);
    }

    private static void ValidateRequest(AppointmentBookRequest request)
    {
        if (request.ServiceIds is null)
        {
            throw new BadRequestException("At least one service is required.");
        }

        ValidateServiceIds(request.ServiceIds);
    }

    private static string NormalizePaymentMethod(string? paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
        {
            throw new BadRequestException("Payment method is required.");
        }

        var normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();
        if (!PaymentMethods.OnSite.Contains(normalizedPaymentMethod))
        {
            throw new BadRequestException("Payment method is not valid.");
        }

        return normalizedPaymentMethod;
    }

    private static void ValidateServiceIds(IReadOnlyCollection<int> serviceIds)
    {
        if (serviceIds.Count == 0)
        {
            throw new BadRequestException("At least one service is required.");
        }

        if (serviceIds.Any(serviceId => serviceId <= 0))
        {
            throw new BadRequestException("Service ids must be valid.");
        }

        if (serviceIds.Distinct().Count() != serviceIds.Count)
        {
            throw new BadRequestException("Duplicate services are not allowed.");
        }
    }

    private static void ValidateStartTime(DateTime startTime)
    {
        if (startTime == default)
        {
            throw new BadRequestException("Start time is required.");
        }

        if (startTime <= DateTime.UtcNow)
        {
            throw new BadRequestException("Start time must be in the future.");
        }
    }

    private static void ValidateServices(IReadOnlyCollection<int> requestedServiceIds, IReadOnlyCollection<ServiceEntity> services, int shopId)
    {
        if (services.Count != requestedServiceIds.Count)
        {
            throw new BadRequestException("One or more services do not exist.");
        }

        if (services.Any(service => !service.Active))
        {
            throw new BadRequestException("One or more services are not active.");
        }

        if (services.Any(service => service.ShopId != shopId))
        {
            throw new BadRequestException("One or more services do not belong to the employee shop.");
        }
    }

    private static void ValidateCalculatedRange(DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
        {
            throw new BadRequestException("Calculated appointment end time must be after start time.");
        }

        if (startTime.Date != endTime.Date)
        {
            throw new BadRequestException("Appointment must fit within a single working day.");
        }
    }

    private async Task EnsureWithinWorkingHoursAsync(int employeeId, DateTime startTime, DateTime endTime)
    {
        var dayOfWeek = (int)startTime.DayOfWeek;
        var startOnly = TimeOnly.FromDateTime(startTime);
        var endOnly = TimeOnly.FromDateTime(endTime);

        var withinWorkingHours = await _dbContext.WorkingHours.AnyAsync(workingHour =>
            workingHour.EmployeeId == employeeId
            && workingHour.DayOfWeek == dayOfWeek
            && workingHour.Active
            && startOnly >= workingHour.StartTime
            && endOnly <= workingHour.EndTime);

        if (!withinWorkingHours)
        {
            throw new BadRequestException("Appointment is outside employee working hours.");
        }
    }

    private async Task EnsureClientActiveFutureAppointmentLimitAsync(int clientId, int shopId)
    {
        var activeFutureAppointmentCount = await _dbContext.Appointments.CountAsync(appointment =>
            appointment.ClientId == clientId
            && appointment.Employee.ShopId == shopId
            && AppointmentStatuses.Blocking.Contains(appointment.Status)
            && appointment.StartTime > DateTime.UtcNow);

        if (activeFutureAppointmentCount >= 3)
        {
            throw new BadRequestException("You cannot have more than 3 active upcoming appointments in this shop.");
        }
    }

    private async Task EnsureNoTimeOffOverlapAsync(int employeeId, DateTime startTime, DateTime endTime)
    {
        var overlaps = await _dbContext.TimeOffs.AnyAsync(timeOff =>
            timeOff.EmployeeId == employeeId
            && timeOff.Status == TimeOffStatuses.APPROVED
            && startTime < timeOff.EndTime
            && endTime > timeOff.StartTime);

        if (overlaps)
        {
            throw new BadRequestException("Appointment overlaps with employee time off.");
        }
    }

    private async Task EnsureAppointmentSlotAvailableAsync(
        int employeeId,
        int shopId,
        DateTime startTime,
        DateTime endTime,
        IReadOnlyCollection<ServiceEntity> services)
    {
        var overlappingAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.AppointmentServices)
            .ThenInclude(appointmentService => appointmentService.Service)
            .Where(appointment => appointment.EmployeeId == employeeId
                && appointment.Employee.ShopId == shopId
                && (appointment.Status.ToUpper() == AppointmentStatuses.PENDING
                    || appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED)
                && startTime < appointment.EndTime
                && endTime > appointment.StartTime)
            .ToListAsync();

        if (!AppointmentOverlapPolicy.IsSlotAvailable(services, overlappingAppointments))
        {
            throw new BadRequestException("Appointment slot is not available for the selected service overlap settings.");
        }
    }

    private static List<ServiceEntity> OrderByRequest(IEnumerable<int> requestedIds, IReadOnlyCollection<ServiceEntity> services)
    {
        var servicesById = services.ToDictionary(service => service.Id);
        return requestedIds.Select(serviceId => servicesById[serviceId]).ToList();
    }

    private static AppointmentDto MapToDto(
        Appointment appointment,
        IReadOnlyCollection<ServiceEntity> services,
        string? paymentMethod)
    {
        var servicesById = services.ToDictionary(service => service.Id);

        return new AppointmentDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            EmployeeId = appointment.EmployeeId,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            TotalPrice = appointment.TotalPrice,
            PaymentMethod = paymentMethod,
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            Services = appointment.AppointmentServices.Select(appointmentService => new AppointmentServiceDto
            {
                ServiceId = appointmentService.ServiceId,
                ServiceName = servicesById[appointmentService.ServiceId].Name,
                PriceAtBooking = appointmentService.PriceAtBooking,
                DurationAtBooking = appointmentService.DurationAtBooking
            }).ToList()
        };
    }

    private static DateTime ToUtcDateTime(DateTimeOffset dateTime)
    {
        return dateTime.UtcDateTime;
    }
}
