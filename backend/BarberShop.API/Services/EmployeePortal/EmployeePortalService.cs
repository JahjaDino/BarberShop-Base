using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.EmployeePortal;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.EmployeePortal;
using BarberShop.API.Services.Notifications;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.EmployeePortal;

public class EmployeePortalService : IEmployeePortalService
{
    private const string DayStatusAvailable = "AVAILABLE";
    private const string DayStatusBusy = "BUSY";
    private const string DayStatusAbsent = "ABSENT";

    private const string ItemTypeShiftStart = "SHIFT_START";
    private const string ItemTypeAppointment = "APPOINTMENT";
    private const string ItemTypeShiftEnd = "SHIFT_END";
    private const string ItemTypeInfo = "INFO";

    private readonly BarberShopDbContext _dbContext;
    private readonly IEmployeeAccessService _employeeAccessService;
    private readonly INotificationService _notificationService;

    public EmployeePortalService(
        BarberShopDbContext dbContext,
        IEmployeeAccessService employeeAccessService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _employeeAccessService = employeeAccessService;
        _notificationService = notificationService;
    }

    public async Task<EmployeeDashboardDto> GetDashboardAsync(DateOnly? date)
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();
        var now = DateTime.UtcNow;
        var today = date ?? DateOnly.FromDateTime(DateTime.Now);
        var (todayStart, tomorrowStart) = GetUtcDayRange(today);

        var workingHoursToday = await GetWorkingHoursForDateAsync(employee.Id, today);
        var todayAppointments = await EmployeeAppointments(employee.Id)
            .Where(appointment => appointment.StartTime >= todayStart && appointment.StartTime < tomorrowStart)
            .Where(appointment => appointment.Status.ToUpper() == AppointmentStatuses.PENDING
                || appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED)
            .OrderBy(appointment => appointment.StartTime)
            .ToListAsync();

        var nextAppointment = await EmployeeAppointments(employee.Id)
            .Where(appointment => (appointment.Status.ToUpper() == AppointmentStatuses.PENDING
                    || appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED)
                && appointment.StartTime > now)
            .OrderBy(appointment => appointment.StartTime)
            .FirstOrDefaultAsync();

        var isCurrentDay = today == DateOnly.FromDateTime(DateTime.Now);
        var dayStatus = await GetDayStatusAsync(employee.Id, todayStart, tomorrowStart, now, isCurrentDay);
        var timeOffSummary = await GetTimeOffSummaryAsync(employee.Id, now);

        return new EmployeeDashboardDto
        {
            Summary = new EmployeeDashboardSummaryDto
            {
                TodayAppointmentsCount = todayAppointments.Count,
                ConfirmedTodayAppointmentsCount = todayAppointments.Count(appointment => NormalizeStatus(appointment.Status) == AppointmentStatuses.CONFIRMED),
                NextAppointmentTime = nextAppointment?.StartTime,
                NextAppointmentServiceName = nextAppointment is null ? null : GetServiceName(nextAppointment),
                WorkingHoursToday = workingHoursToday,
                DayStatus = dayStatus
            },
            TodaySchedule = BuildScheduleItems(today, workingHoursToday, todayAppointments),
            AssignedAppointments = todayAppointments
                .Where(appointment => AppointmentStatuses.Blocking.Contains(NormalizeStatus(appointment.Status)))
                .Select(appointment => new EmployeeAssignedAppointmentDto
                {
                    AppointmentId = appointment.Id,
                    ClientName = GetClientName(appointment),
                    ServiceName = GetServiceName(appointment),
                    Time = appointment.StartTime.ToString("HH:mm"),
                    Status = NormalizeStatus(appointment.Status)
                })
                .ToList(),
            TimeOffSummary = timeOffSummary
        };
    }

    public async Task<EmployeeScheduleDto> GetScheduleAsync(DateOnly? date)
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();
        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var (dayStart, dayEnd) = GetUtcDayRange(scheduleDate);

        var workingHours = await GetWorkingHoursForDateAsync(employee.Id, scheduleDate);
        var appointments = await EmployeeAppointments(employee.Id)
            .Where(appointment => appointment.StartTime >= dayStart && appointment.StartTime < dayEnd)
            .Where(appointment => appointment.Status.ToUpper() == AppointmentStatuses.PENDING
                || appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED)
            .OrderBy(appointment => appointment.StartTime)
            .ToListAsync();

        return new EmployeeScheduleDto
        {
            Date = scheduleDate,
            WorkingHours = workingHours,
            Items = BuildScheduleItems(scheduleDate, workingHours, appointments)
        };
    }

    public async Task<PagedResult<EmployeeAppointmentListItemDto>> GetAppointmentsAsync(EmployeeAppointmentSearchObject search)
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();
        var query = EmployeeAppointments(employee.Id);

        if (search.Date.HasValue)
        {
            var (dayStart, dayEnd) = GetUtcDayRange(search.Date.Value);
            query = query.Where(appointment => appointment.StartTime >= dayStart && appointment.StartTime < dayEnd);
        }

        if (!string.IsNullOrWhiteSpace(search.Status))
        {
            var status = search.Status.Trim().ToUpperInvariant();
            if (!AppointmentStatuses.All.Contains(status))
            {
                throw new BadRequestException("Appointment status is not valid.");
            }

            query = query.Where(appointment => appointment.Status.ToUpper() == status);
        }

        query = query.OrderBy(appointment => appointment.StartTime);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<IReadOnlyCollection<EmployeeTimeOffDto>> GetTimeOffAsync()
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();

        return await _dbContext.TimeOffs
            .AsNoTracking()
            .Where(timeOff => timeOff.EmployeeId == employee.Id)
            .OrderByDescending(timeOff => timeOff.StartTime)
            .Select(timeOff => new EmployeeTimeOffDto
            {
                TimeOffId = timeOff.Id,
                StartDate = timeOff.StartTime,
                EndDate = timeOff.EndTime,
                Reason = timeOff.Reason,
                Status = timeOff.Status,
                ReviewedAt = timeOff.ReviewedAt,
                ReviewNote = timeOff.ReviewNote,
                ReviewedByName = timeOff.ReviewedByUser == null
                    ? null
                    : timeOff.ReviewedByUser.FirstName + " " + timeOff.ReviewedByUser.LastName
            })
            .ToListAsync();
    }

    public async Task<EmployeeTimeOffDto> CreateTimeOffAsync(EmployeeTimeOffCreateRequest request)
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();
        var startTime = request.StartTime.UtcDateTime;
        var endTime = request.EndTime.UtcDateTime;
        var now = DateTime.UtcNow;

        if (startTime < now)
        {
            throw new BadRequestException("Time off cannot start in the past.");
        }

        if (startTime >= endTime)
        {
            throw new BadRequestException("Time off start time must be before end time.");
        }

        var overlapsExistingTimeOff = await _dbContext.TimeOffs
            .AsNoTracking()
            .AnyAsync(timeOff => timeOff.EmployeeId == employee.Id
                && !TimeOffStatuses.NonBlocking.Contains(timeOff.Status)
                && startTime < timeOff.EndTime
                && endTime > timeOff.StartTime);
        if (overlapsExistingTimeOff)
        {
            throw new ConflictException("Time off overlaps with an existing request.");
        }

        var timeOff = new Entities.TimeOff
        {
            EmployeeId = employee.Id,
            StartTime = startTime,
            EndTime = endTime,
            Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            Status = TimeOffStatuses.PENDING
        };

        _dbContext.TimeOffs.Add(timeOff);
        await _dbContext.SaveChangesAsync();
        await _notificationService.CreateForTimeOffRequestedAsync(timeOff.Id);

        return new EmployeeTimeOffDto
        {
            TimeOffId = timeOff.Id,
            StartDate = timeOff.StartTime,
            EndDate = timeOff.EndTime,
            Reason = timeOff.Reason,
            Status = timeOff.Status,
            ReviewedAt = timeOff.ReviewedAt,
            ReviewNote = timeOff.ReviewNote
        };
    }

    public async Task<IReadOnlyCollection<EmployeeServiceDto>> GetServicesAsync()
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();

        return await _dbContext.Services
            .AsNoTracking()
            .Where(service => service.ShopId == employee.ShopId && service.Active)
            .OrderBy(service => service.Category.Name)
            .ThenBy(service => service.Name)
            .Select(service => new EmployeeServiceDto
            {
                ServiceId = service.Id,
                Name = service.Name,
                Description = service.Description,
                CategoryName = service.Category.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<EmployeeInventoryItemDto>> GetInventoryAsync()
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();

        return await _dbContext.InventoryItems
            .AsNoTracking()
            .Where(item => item.ShopId == employee.ShopId)
            .OrderBy(item => item.Name)
            .Select(item => new EmployeeInventoryItemDto
            {
                ItemId = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                MinimumQuantity = item.MinimumQuantity,
                Status = item.Quantity <= 0
                    ? "OUT_OF_STOCK"
                    : item.Quantity <= item.MinimumQuantity
                        ? "LOW_STOCK"
                        : "OK",
                LastUpdated = item.LastUpdated,
                ReportNote = item.ReportNote
            })
            .ToListAsync();
    }

    public async Task<EmployeeInventoryItemDto> ReportInventoryAsync(EmployeeInventoryReportRequest request)
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();
        var name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Inventory item name is required.");
        }

        var unit = string.IsNullOrWhiteSpace(request.Unit) ? "kom" : request.Unit.Trim();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();
            if (status is not ("OK" or "LOW_STOCK" or "OUT_OF_STOCK"))
            {
                throw new BadRequestException("Inventory status is not valid.");
            }
        }

        var normalizedName = name.ToLower();
        var item = await _dbContext.InventoryItems
            .FirstOrDefaultAsync(currentItem =>
                currentItem.ShopId == employee.ShopId
                && currentItem.Name.ToLower() == normalizedName);

        if (item is null)
        {
            item = new InventoryItem
            {
                ShopId = employee.ShopId,
                Name = name
            };
            _dbContext.InventoryItems.Add(item);
        }

        item.Quantity = request.Quantity;
        item.MinimumQuantity = request.MinimumQuantity;
        item.Unit = unit;
        item.LastUpdated = DateTime.UtcNow;
        item.ReportedByEmployeeId = employee.Id;
        item.ReportedAt = item.LastUpdated;
        item.ReportNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        await _dbContext.SaveChangesAsync();

        return MapInventoryItem(item);
    }

    public async Task<EmployeeProfileDto> GetProfileAsync()
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();

        return await GetProfileForEmployeeAsync(employee.Id);
    }

    public async Task<EmployeeProfileDto> UpdateProfileAsync(EmployeeProfileUpdateRequest request)
    {
        var employee = await GetCurrentEmployeeOrThrowAsync();
        var user = await _dbContext.Users.FirstAsync(currentUser => currentUser.Id == employee.UserId);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.PhoneNumber.Trim();

        await _dbContext.SaveChangesAsync();

        return await GetProfileForEmployeeAsync(employee.Id);
    }

    private async Task<Employee> GetCurrentEmployeeOrThrowAsync()
    {
        var employee = await _employeeAccessService.GetCurrentEmployeeAsync();
        if (employee is null)
        {
            throw new ForbiddenException("Current user does not have an active employee profile.");
        }

        return employee;
    }

    private IQueryable<Appointment> EmployeeAppointments(int employeeId)
    {
        return _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.EmployeeId == employeeId)
            .Include(appointment => appointment.Client)
            .ThenInclude(client => client.User)
            .Include(appointment => appointment.AppointmentServices)
            .ThenInclude(appointmentService => appointmentService.Service);
    }

    private async Task<IReadOnlyCollection<EmployeeScheduleWorkingHoursDto>> GetWorkingHoursForDateAsync(
        int employeeId,
        DateOnly date)
    {
        return await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(workingHour => workingHour.EmployeeId == employeeId
                && workingHour.Active
                && workingHour.DayOfWeek == (int)date.DayOfWeek)
            .OrderBy(workingHour => workingHour.StartTime)
            .Select(workingHour => new EmployeeScheduleWorkingHoursDto
            {
                StartTime = workingHour.StartTime.ToString("HH:mm"),
                EndTime = workingHour.EndTime.ToString("HH:mm")
            })
            .ToListAsync();
    }

    private async Task<string> GetDayStatusAsync(
        int employeeId,
        DateTime todayStart,
        DateTime tomorrowStart,
        DateTime now,
        bool isCurrentDay)
    {
        var hasTimeOffToday = await _dbContext.TimeOffs
            .AsNoTracking()
            .AnyAsync(timeOff => timeOff.EmployeeId == employeeId
                && timeOff.Status == TimeOffStatuses.APPROVED
                && timeOff.StartTime < tomorrowStart
                && timeOff.EndTime > todayStart);
        if (hasTimeOffToday)
        {
            return DayStatusAbsent;
        }

        var hasActiveAppointment = isCurrentDay && await _dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(appointment => appointment.EmployeeId == employeeId
                && (appointment.Status.ToUpper() == AppointmentStatuses.PENDING
                    || appointment.Status.ToUpper() == AppointmentStatuses.CONFIRMED)
                && appointment.StartTime <= now
                && appointment.EndTime > now);

        return hasActiveAppointment ? DayStatusBusy : DayStatusAvailable;
    }

    private async Task<EmployeeTimeOffSummaryDto> GetTimeOffSummaryAsync(int employeeId, DateTime now)
    {
        var activeTimeOffQuery = _dbContext.TimeOffs
            .AsNoTracking()
            .Where(timeOff => timeOff.EmployeeId == employeeId
                && timeOff.Status == TimeOffStatuses.APPROVED
                && timeOff.EndTime > now);

        return new EmployeeTimeOffSummaryDto
        {
            HasActiveTimeOff = await activeTimeOffQuery.AnyAsync(timeOff => timeOff.StartTime <= now && timeOff.EndTime > now),
            ActiveTimeOffCount = await activeTimeOffQuery.CountAsync(),
            PendingTimeOffCount = await _dbContext.TimeOffs
                .AsNoTracking()
                .CountAsync(timeOff => timeOff.EmployeeId == employeeId && timeOff.Status == TimeOffStatuses.PENDING),
            NextTimeOffDate = await activeTimeOffQuery
                .Where(timeOff => timeOff.StartTime > now)
                .OrderBy(timeOff => timeOff.StartTime)
                .Select(timeOff => (DateTime?)timeOff.StartTime)
                .FirstOrDefaultAsync()
        };
    }

    private async Task<EmployeeProfileDto> GetProfileForEmployeeAsync(int employeeId)
    {
        var profile = await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.Id == employeeId)
            .Select(employee => new EmployeeProfileDto
            {
                EmployeeId = employee.Id,
                FirstName = employee.User.FirstName,
                LastName = employee.User.LastName,
                Email = employee.User.Email,
                PhoneNumber = employee.User.Phone,
                Position = employee.Position,
                ShopName = employee.Shop.Name
            })
            .FirstAsync();

        profile.WorkingHoursSummary = await _dbContext.WorkingHours
            .AsNoTracking()
            .Where(workingHour => workingHour.EmployeeId == employeeId)
            .OrderBy(workingHour => workingHour.DayOfWeek)
            .ThenBy(workingHour => workingHour.StartTime)
            .Select(workingHour => new EmployeeWorkingHoursSummaryDto
            {
                DayOfWeek = workingHour.DayOfWeek,
                StartTime = workingHour.StartTime.ToString("HH:mm"),
                EndTime = workingHour.EndTime.ToString("HH:mm"),
                Active = workingHour.Active
            })
            .ToListAsync();

        return profile;
    }

    private static IReadOnlyCollection<EmployeeScheduleItemDto> BuildScheduleItems(
        DateOnly date,
        IReadOnlyCollection<EmployeeScheduleWorkingHoursDto> workingHours,
        IReadOnlyCollection<Appointment> appointments)
    {
        var items = new List<EmployeeScheduleItemDto>();

        if (workingHours.Count == 0)
        {
            return appointments
                .Select(appointment => new EmployeeScheduleItemDto
                {
                    Time = appointment.StartTime.ToString("HH:mm"),
                    Title = GetServiceName(appointment),
                    Description = appointment.Notes,
                    ClientName = GetClientName(appointment),
                    AppointmentId = appointment.Id,
                    Type = ItemTypeAppointment,
                    Status = NormalizeStatus(appointment.Status)
                })
                .OrderBy(item => ToSortableTime(date, item.Time))
                .ToList();
        }

        foreach (var workingHour in workingHours)
        {
            items.Add(new EmployeeScheduleItemDto
            {
                Time = workingHour.StartTime,
                Title = "Početak smjene",
                Type = ItemTypeShiftStart
            });

            items.Add(new EmployeeScheduleItemDto
            {
                Time = workingHour.EndTime,
                Title = "Kraj smjene",
                Type = ItemTypeShiftEnd
            });
        }

        items.AddRange(appointments.Select(appointment => new EmployeeScheduleItemDto
        {
            Time = appointment.StartTime.ToString("HH:mm"),
            Title = GetServiceName(appointment),
            Description = appointment.Notes,
            ClientName = GetClientName(appointment),
            AppointmentId = appointment.Id,
            Type = ItemTypeAppointment,
            Status = NormalizeStatus(appointment.Status)
        }));

        return items
            .OrderBy(item => ToSortableTime(date, item.Time))
            .ThenBy(item => item.Type == ItemTypeAppointment ? 1 : 0)
            .ToList();
    }

    private static DateTime ToSortableTime(DateOnly date, string time)
    {
        return TimeOnly.TryParse(time, out var parsedTime)
            ? date.ToDateTime(parsedTime)
            : date.ToDateTime(TimeOnly.MinValue);
    }

    private static string GetClientName(Appointment appointment)
    {
        return appointment.Client.User.FirstName + " " + appointment.Client.User.LastName;
    }

    private static string GetServiceName(Appointment appointment)
    {
        return string.Join(", ", appointment.AppointmentServices
            .OrderBy(appointmentService => appointmentService.Id)
            .Select(appointmentService => appointmentService.Service.Name)
            .Where(serviceName => !string.IsNullOrWhiteSpace(serviceName)));
    }

    private static int GetDurationMinutes(Appointment appointment)
    {
        return appointment.AppointmentServices.Sum(appointmentService => appointmentService.DurationAtBooking);
    }

    private static (DateTime Start, DateTime End) GetUtcDayRange(DateOnly date)
    {
        var start = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        return (start, start.AddDays(1));
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToUpperInvariant();
    }

    private static string GetInventoryStatus(InventoryItem item)
    {
        if (item.Quantity <= 0)
        {
            return "OUT_OF_STOCK";
        }

        return item.Quantity <= item.MinimumQuantity ? "LOW_STOCK" : "OK";
    }

    private static EmployeeInventoryItemDto MapInventoryItem(InventoryItem item)
    {
        return new EmployeeInventoryItemDto
        {
            ItemId = item.Id,
            Name = item.Name,
            Quantity = item.Quantity,
            Unit = item.Unit,
            MinimumQuantity = item.MinimumQuantity,
            Status = GetInventoryStatus(item),
            LastUpdated = item.LastUpdated,
            ReportNote = item.ReportNote
        };
    }

    private static EmployeeAppointmentListItemDto MapAppointmentListItem(Appointment appointment)
    {
        return new EmployeeAppointmentListItemDto
        {
            AppointmentId = appointment.Id,
            Date = DateOnly.FromDateTime(appointment.StartTime),
            Time = appointment.StartTime.ToString("HH:mm"),
            ClientName = GetClientName(appointment),
            ServiceName = GetServiceName(appointment),
            DurationMinutes = GetDurationMinutes(appointment),
            Price = appointment.TotalPrice,
            Status = NormalizeStatus(appointment.Status)
        };
    }

    private static async Task<PagedResult<EmployeeAppointmentListItemDto>> ToPagedResultAsync(
        IQueryable<Appointment> query,
        BaseSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = search.PageSize <= 0
            ? BaseSearchObject.DefaultPageSize
            : Math.Min(search.PageSize, BaseSearchObject.MaxPageSize);

        var totalCount = search.IncludeTotalCount ? await query.CountAsync() : 0;
        var appointments = await (search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize)).ToListAsync();

        return new PagedResult<EmployeeAppointmentListItemDto>
        {
            Items = appointments.Select(MapAppointmentListItem).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0
        };
    }
}
