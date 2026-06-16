using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Appointments;
using BarberShop.API.DTOs.Client;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.Appointments;
using BarberShop.API.Services.Notifications;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Appointments;

public class AppointmentManagementService : IAppointmentManagementService
{
    private static readonly TimeSpan ClientCancellationWindow = TimeSpan.FromHours(24);
    private static readonly TimeSpan SlotStep = TimeSpan.FromMinutes(15);

    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOwnerAccessService _ownerAccessService;
    private readonly IEmployeeAccessService _employeeAccessService;
    private readonly INotificationService _notificationService;

    public AppointmentManagementService(
        BarberShopDbContext dbContext,
        ICurrentUserService currentUserService,
        IOwnerAccessService ownerAccessService,
        IEmployeeAccessService employeeAccessService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _ownerAccessService = ownerAccessService;
        _employeeAccessService = employeeAccessService;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<AppointmentDto>> GetAsync(AppointmentSearchObject search)
    {
        var ownerShopId = await _ownerAccessService.GetOwnerShopIdAsync();
        if (!ownerShopId.HasValue)
        {
            throw new ForbiddenException("You do not have access to appointments.");
        }

        var query = BaseQuery()
            .Where(appointment => appointment.Employee.ShopId == ownerShopId.Value);

        query = AddFilter(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<PagedResult<AppointmentDto>> GetMineAsync(AppointmentSearchObject search)
    {
        var clientId = await GetCurrentClientIdAsync();

        var query = BaseQuery()
            .Where(appointment => appointment.ClientId == clientId);

        query = AddFilter(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<PagedResult<ClientAppointmentCardDto>> GetMyClientCardsAsync(ClientAppointmentSearchObject search)
    {
        var clientId = await GetCurrentClientIdAsync();
        var now = DateTime.UtcNow;

        var query = BaseQuery()
            .Where(appointment => appointment.ClientId == clientId);

        query = ApplyClientAppointmentFilter(query, search.Filter, now);
        query = query.OrderBy(appointment => appointment.StartTime);

        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);
        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var appointments = await query.ToListAsync();

        return new PagedResult<ClientAppointmentCardDto>
        {
            Items = appointments.Select(MapToClientCardDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    public async Task<PagedResult<AppointmentDto>> GetMyEmployeeAppointmentsAsync(AppointmentSearchObject search)
    {
        var currentEmployee = await _employeeAccessService.GetCurrentEmployeeAsync();
        if (currentEmployee is null)
        {
            throw new ForbiddenException("Current user does not have an active employee profile.");
        }

        var query = BaseQuery()
            .Where(appointment => appointment.EmployeeId == currentEmployee.Id);

        query = AddFilter(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await BaseQuery()
            .FirstOrDefaultAsync(currentAppointment => currentAppointment.Id == id);
        if (appointment is null)
        {
            return null;
        }

        await EnsureCanAccessAppointmentAsync(appointment);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto> CancelAsync(int id, AppointmentCancelRequest request)
    {
        var appointment = await TrackedQuery()
            .FirstOrDefaultAsync(currentAppointment => currentAppointment.Id == id);
        if (appointment is null)
        {
            throw new NotFoundException("Appointment does not exist.");
        }

        var access = await GetAppointmentAccessAsync(appointment);
        EnsureCanCancel(appointment, access);

        await ChangeStatusAsync(appointment, AppointmentStatuses.CANCELLED, request.Reason);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, AppointmentStatusUpdateRequest request)
    {
        var appointment = await TrackedQuery()
            .FirstOrDefaultAsync(currentAppointment => currentAppointment.Id == id);
        if (appointment is null)
        {
            return null;
        }

        await EnsureOwnerOrEmployeeCanManageAppointmentAsync(appointment);

        var newStatus = MapStatusUpdateType(request);
        EnsureTransitionIsAllowed(appointment.Status, newStatus);

        await ChangeStatusAsync(appointment, newStatus, null);

        return MapToDto(appointment);
    }

    public async Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsAsync(int serviceId, int employeeId, DateOnly date)
    {
        return await GetAvailableSlotsCoreAsync(null, serviceId, employeeId, date);
    }

    public async Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsAsync(
        int shopId,
        int serviceId,
        int employeeId,
        DateOnly date)
    {
        return await GetAvailableSlotsCoreAsync(shopId, serviceId, employeeId, date);
    }

    private async Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsCoreAsync(
        int? shopId,
        int serviceId,
        int employeeId,
        DateOnly date)
    {
        var service = await _dbContext.Services
            .AsNoTracking()
            .Where(currentService => currentService.Id == serviceId)
            .FirstOrDefaultAsync();
        if (service is null)
        {
            throw new BadRequestException("Service does not exist.");
        }

        if (!service.Active)
        {
            throw new BadRequestException("Service is not active.");
        }

        if (shopId.HasValue && service.ShopId != shopId.Value)
        {
            throw new BadRequestException("Service does not belong to the selected shop.");
        }

        var employee = await _dbContext.Employees
            .AsNoTracking()
            .Where(currentEmployee => currentEmployee.Id == employeeId)
            .Select(currentEmployee => new
            {
                currentEmployee.Id,
                currentEmployee.ShopId,
                currentEmployee.Active
            })
            .FirstOrDefaultAsync();
        if (employee is null)
        {
            throw new BadRequestException("Employee does not exist.");
        }

        if (!employee.Active)
        {
            throw new BadRequestException("Employee is not active.");
        }

        if (shopId.HasValue && employee.ShopId != shopId.Value)
        {
            throw new BadRequestException("Employee does not belong to the selected shop.");
        }

        if (employee.ShopId != service.ShopId)
        {
            throw new BadRequestException("Employee and service do not belong to the same shop.");
        }

        var dayStart = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);
        var now = DateTime.UtcNow;

        var workingHours = await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(workingHour => workingHour.EmployeeId == employeeId
                && workingHour.Active
                && workingHour.DayOfWeek == (int)date.DayOfWeek)
            .OrderBy(workingHour => workingHour.StartTime)
            .ToListAsync();

        var timeOffs = await _dbContext.TimeOffs
            .AsNoTracking()
            .Where(timeOff => timeOff.EmployeeId == employeeId
                && timeOff.Status == TimeOffStatuses.APPROVED
                && timeOff.StartTime < dayEnd
                && timeOff.EndTime > dayStart)
            .ToListAsync();

        var blockingAppointments = await _dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.AppointmentServices)
            .ThenInclude(appointmentService => appointmentService.Service)
            .Where(appointment => appointment.EmployeeId == employeeId
                && appointment.Employee.ShopId == employee.ShopId
                && (appointment.Status.ToUpper() == AppointmentStatuses.PENDING
                    || appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED)
                && appointment.StartTime < dayEnd
                && appointment.EndTime > dayStart)
            .ToListAsync();

        var slots = new List<AvailableSlotDto>();
        var duration = TimeSpan.FromMinutes(service.DurationMinutes);

        foreach (var workingHour in workingHours)
        {
            var cursor = DateTime.SpecifyKind(date.ToDateTime(workingHour.StartTime), DateTimeKind.Utc);
            var workingEnd = DateTime.SpecifyKind(date.ToDateTime(workingHour.EndTime), DateTimeKind.Utc);

            while (cursor.Add(duration) <= workingEnd)
            {
                var slotEnd = cursor.Add(duration);
                if (cursor > now
                    && !OverlapsAny(cursor, slotEnd, timeOffs.Select(timeOff => (timeOff.StartTime, timeOff.EndTime)))
                    && AppointmentOverlapPolicy.IsSlotAvailable(
                        new[] { service },
                        blockingAppointments
                            .Where(appointment => cursor < appointment.EndTime && slotEnd > appointment.StartTime)
                            .ToList()))
                {
                    slots.Add(new AvailableSlotDto
                    {
                        StartTime = cursor.ToString("HH:mm"),
                        EndTime = slotEnd.ToString("HH:mm"),
                        Available = true
                    });
                }

                cursor = cursor.Add(SlotStep);
            }
        }

        return slots;
    }

    private IQueryable<Appointment> BaseQuery()
    {
        return _dbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Employee)
            .ThenInclude(employee => employee.User)
            .Include(appointment => appointment.AppointmentServices)
            .ThenInclude(appointmentService => appointmentService.Service)
            .Include(appointment => appointment.Payments)
            .AsQueryable();
    }

    private IQueryable<Appointment> TrackedQuery()
    {
        return _dbContext.Appointments
            .Include(appointment => appointment.Employee)
            .ThenInclude(employee => employee.User)
            .Include(appointment => appointment.AppointmentServices)
            .ThenInclude(appointmentService => appointmentService.Service)
            .Include(appointment => appointment.Payments)
            .AsQueryable();
    }

    private static IQueryable<Appointment> AddFilter(IQueryable<Appointment> query, AppointmentSearchObject search)
    {
        if (search.ClientId.HasValue)
        {
            query = query.Where(appointment => appointment.ClientId == search.ClientId.Value);
        }

        if (search.EmployeeId.HasValue)
        {
            query = query.Where(appointment => appointment.EmployeeId == search.EmployeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
        {
            var status = search.Status.Trim().ToUpper();
            query = query.Where(appointment => appointment.Status.ToUpper() == status);
        }

        if (search.DateFrom.HasValue)
        {
            var dateFrom = ToUtcDateTime(search.DateFrom.Value);
            query = query.Where(appointment => appointment.EndTime >= dateFrom);
        }

        if (search.DateTo.HasValue)
        {
            var dateTo = ToUtcDateTime(search.DateTo.Value);
            query = query.Where(appointment => appointment.StartTime <= dateTo);
        }

        return query.OrderBy(appointment => appointment.StartTime);
    }

    private static async Task<PagedResult<AppointmentDto>> ToPagedResultAsync(
        IQueryable<Appointment> query,
        AppointmentSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);

        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var appointments = await query.ToListAsync();
        var items = appointments.Select(MapToDto).ToList();

        return new PagedResult<AppointmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    private async Task ChangeStatusAsync(Appointment appointment, string newStatus, string? reason)
    {
        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var oldStatus = appointment.Status;
        appointment.Status = newStatus;

        _dbContext.AppointmentStatusHistories.Add(new AppointmentStatusHistory
        {
            AppointmentId = appointment.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedByUserId = currentUserId,
            ChangedAt = DateTime.UtcNow,
            Reason = reason?.Trim()
        });

        await _dbContext.SaveChangesAsync();
        await _notificationService.CreateForAppointmentEventAsync(
            appointment.Id,
            GetNotificationTypeForStatus(newStatus),
            currentUserId);

        await transaction.CommitAsync();
    }

    private async Task EnsureCanAccessAppointmentAsync(Appointment appointment)
    {
        _ = await GetAppointmentAccessAsync(appointment);
    }

    private async Task<AppointmentAccess> GetAppointmentAccessAsync(Appointment appointment)
    {
        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var isOwnerForAppointment = _currentUserService.Roles.Contains(RoleNames.OWNER, StringComparer.OrdinalIgnoreCase)
            && await _ownerAccessService.CanAccessShopAsync(appointment.Employee.ShopId);
        if (isOwnerForAppointment)
        {
            return new AppointmentAccess(currentUserId, true, false);
        }

        var isEmployeeForAppointment = _currentUserService.Roles.Contains(RoleNames.EMPLOYEE, StringComparer.OrdinalIgnoreCase)
            && await _employeeAccessService.CanManageAppointmentAsync(appointment.EmployeeId);
        if (isEmployeeForAppointment)
        {
            return new AppointmentAccess(currentUserId, false, true);
        }

        if (_currentUserService.Roles.Contains(RoleNames.CLIENT, StringComparer.OrdinalIgnoreCase))
        {
            var clientOwnsAppointment = await _dbContext.Clients
                .AsNoTracking()
                .AnyAsync(client => client.UserId == currentUserId && client.Id == appointment.ClientId);
            if (clientOwnsAppointment)
            {
                return new AppointmentAccess(currentUserId, false, false);
            }
        }

        throw new ForbiddenException("You do not have access to this appointment.");
    }

    private async Task EnsureOwnerCanAccessAppointmentAsync(Appointment appointment)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(appointment.Employee.ShopId))
        {
            throw new ForbiddenException("You do not have access to this appointment.");
        }
    }

    private async Task EnsureOwnerOrEmployeeCanManageAppointmentAsync(Appointment appointment)
    {
        var ownerCanAccess = _currentUserService.Roles.Contains(RoleNames.OWNER, StringComparer.OrdinalIgnoreCase)
            && await _ownerAccessService.CanAccessShopAsync(appointment.Employee.ShopId);
        if (ownerCanAccess)
        {
            return;
        }

        var employeeCanAccess = _currentUserService.Roles.Contains(RoleNames.EMPLOYEE, StringComparer.OrdinalIgnoreCase)
            && await _employeeAccessService.CanManageAppointmentAsync(appointment.EmployeeId);
        if (employeeCanAccess)
        {
            return;
        }

        throw new ForbiddenException("You do not have access to this appointment.");
    }

    private async Task<int> GetCurrentClientIdAsync()
    {
        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var clientId = await _dbContext.Clients
            .AsNoTracking()
            .Where(client => client.UserId == currentUserId)
            .Select(client => (int?)client.Id)
            .FirstOrDefaultAsync();

        if (!clientId.HasValue)
        {
            throw new ForbiddenException("Current user does not have a client profile.");
        }

        return clientId.Value;
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            ClientId = appointment.ClientId,
            EmployeeId = appointment.EmployeeId,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Status = appointment.Status,
            TotalPrice = appointment.TotalPrice,
            PaymentMethod = appointment.Payments
                .OrderByDescending(payment => payment.Id)
                .Select(payment => payment.PaymentMethod)
                .FirstOrDefault(),
            Notes = appointment.Notes,
            CreatedAt = appointment.CreatedAt,
            Services = appointment.AppointmentServices.Select(appointmentService => new AppointmentServiceDto
            {
                ServiceId = appointmentService.ServiceId,
                ServiceName = appointmentService.Service?.Name ?? string.Empty,
                PriceAtBooking = appointmentService.PriceAtBooking,
                DurationAtBooking = appointmentService.DurationAtBooking
            }).ToList()
        };
    }

    private static ClientAppointmentCardDto MapToClientCardDto(Appointment appointment)
    {
        return new ClientAppointmentCardDto
        {
            AppointmentId = appointment.Id,
            ServiceName = string.Join(", ", appointment.AppointmentServices
                .OrderBy(appointmentService => appointmentService.Id)
                .Select(appointmentService => appointmentService.Service?.Name ?? string.Empty)
                .Where(serviceName => !string.IsNullOrWhiteSpace(serviceName))),
            EmployeeName = appointment.Employee.User.FirstName + " " + appointment.Employee.User.LastName,
            Date = DateOnly.FromDateTime(appointment.StartTime),
            Time = TimeOnly.FromDateTime(appointment.StartTime),
            DurationMinutes = appointment.AppointmentServices.Sum(appointmentService => appointmentService.DurationAtBooking),
            Price = appointment.TotalPrice,
            PaymentMethod = appointment.Payments
                .OrderByDescending(payment => payment.Id)
                .Select(payment => payment.PaymentMethod)
                .FirstOrDefault(),
            Status = appointment.Status
        };
    }

    private static IQueryable<Appointment> ApplyClientAppointmentFilter(
        IQueryable<Appointment> query,
        string? filter,
        DateTime now)
    {
        return filter?.Trim().ToLowerInvariant() switch
        {
            "upcoming" => query.Where(appointment =>
                AppointmentStatuses.Blocking.Contains(appointment.Status) && appointment.StartTime > now),
            "completed" => query.Where(appointment => appointment.Status == AppointmentStatuses.COMPLETED),
            "cancelled" => query.Where(appointment =>
                appointment.Status == AppointmentStatuses.CANCELLED || appointment.Status == AppointmentStatuses.NO_SHOW),
            null or "" => query,
            _ => throw new BadRequestException("Appointment filter is not valid.")
        };
    }

    private static bool OverlapsAny(
        DateTime start,
        DateTime end,
        IEnumerable<(DateTime StartTime, DateTime EndTime)> intervals)
    {
        return intervals.Any(interval => start < interval.EndTime && end > interval.StartTime);
    }

    private static void EnsureCanCancel(Appointment appointment, AppointmentAccess access)
    {
        EnsureTransitionIsAllowed(appointment.Status, AppointmentStatuses.CANCELLED);

        if (!access.IsOwnerForAppointment
            && !access.IsEmployeeForAppointment
            && appointment.StartTime <= DateTime.UtcNow.Add(ClientCancellationWindow))
        {
            throw new BadRequestException("Appointment cannot be cancelled less than 24 hours before start time.");
        }
    }

    private static void EnsureTransitionIsAllowed(string currentStatus, string newStatus)
    {
        if (string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException("Appointment is already in the requested status.");
        }

        if (AppointmentStatuses.Terminal.Contains(currentStatus))
        {
            throw new BadRequestException("Appointment status cannot be changed from a terminal status.");
        }

        if (!AppointmentStatuses.CanTransition(currentStatus, newStatus))
        {
            throw new BadRequestException($"Appointment status cannot be changed from {currentStatus} to {newStatus}.");
        }
    }

    private static string MapStatusUpdateType(AppointmentStatusUpdateRequest request)
    {
        if (request is null)
        {
            throw new BadRequestException("Status update request is required.");
        }

        return request.NewStatus switch
        {
            AppointmentStatusUpdateType.CONFIRMED => AppointmentStatuses.CONFIRMED,
            AppointmentStatusUpdateType.COMPLETED => AppointmentStatuses.COMPLETED,
            AppointmentStatusUpdateType.NO_SHOW => AppointmentStatuses.NO_SHOW,
            _ => throw new BadRequestException("Status is not valid.")
        };
    }

    private static string GetNotificationTypeForStatus(string status)
    {
        return status switch
        {
            AppointmentStatuses.CONFIRMED => NotificationTypes.APPOINTMENT_CONFIRMED,
            AppointmentStatuses.CANCELLED => NotificationTypes.APPOINTMENT_CANCELLED,
            AppointmentStatuses.COMPLETED => NotificationTypes.APPOINTMENT_COMPLETED,
            AppointmentStatuses.NO_SHOW => NotificationTypes.APPOINTMENT_NO_SHOW,
            _ => throw new BadRequestException("Notification type cannot be resolved for appointment status.")
        };
    }

    private sealed record AppointmentAccess(int CurrentUserId, bool IsOwnerForAppointment, bool IsEmployeeForAppointment);

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return BaseSearchObject.DefaultPageSize;
        }

        return Math.Min(pageSize, BaseSearchObject.MaxPageSize);
    }

    private static DateTime ToUtcDateTime(DateTimeOffset dateTime)
    {
        return dateTime.UtcDateTime;
    }
}
