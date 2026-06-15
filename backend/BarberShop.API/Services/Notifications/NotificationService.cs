using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Notifications;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.Notifications;
using BarberShop.API.Services.Email;
using BarberShop.API.Services.Notifications.Factories;
using BarberShop.API.Services.Notifications.Strategies;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationStrategyFactory _strategyFactory;
    private readonly IEmailService _emailService;

    public NotificationService(
        BarberShopDbContext dbContext,
        ICurrentUserService currentUserService,
        INotificationStrategyFactory strategyFactory,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _strategyFactory = strategyFactory;
        _emailService = emailService;
    }

    public async Task CreateForAppointmentEventAsync(int appointmentId, string type, int triggeredByUserId)
    {
        var strategy = _strategyFactory.GetStrategy(type);
        var context = await BuildAppointmentContextAsync(appointmentId, triggeredByUserId);
        var notifications = strategy.CreateNotifications(context)
            .Where(notification => notification.UserId > 0)
            .ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        _dbContext.Notifications.AddRange(notifications);
        await _dbContext.SaveChangesAsync();

        var emailMessages = strategy.CreateEmailMessages(context)
            .Where(message => !string.IsNullOrWhiteSpace(message.To))
            .ToList();

        foreach (var emailMessage in emailMessages)
        {
            await _emailService.SendAsync(emailMessage);
        }
    }

    public async Task CreateForTimeOffRequestedAsync(int timeOffId)
    {
        var timeOff = await _dbContext.TimeOffs
            .AsNoTracking()
            .Where(currentTimeOff => currentTimeOff.Id == timeOffId)
            .Select(currentTimeOff => new
            {
                currentTimeOff.Id,
                currentTimeOff.Employee.ShopId,
                EmployeeName = currentTimeOff.Employee.User.FirstName + " " + currentTimeOff.Employee.User.LastName
            })
            .FirstOrDefaultAsync();
        if (timeOff is null)
        {
            throw new BadRequestException("Time off request does not exist.");
        }

        var ownerUserIds = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole =>
                userRole.ShopId == timeOff.ShopId
                && userRole.Role.Name.ToUpper() == RoleNames.OWNER)
            .Select(userRole => userRole.UserId)
            .Distinct()
            .ToListAsync();

        var notifications = ownerUserIds.Select(ownerUserId => new Notification
        {
            UserId = ownerUserId,
            TimeOffId = timeOff.Id,
            Type = NotificationTypes.TIME_OFF_REQUESTED,
            Message = $"Novi zahtjev za odsustvo: {timeOff.EmployeeName}.",
            Status = NotificationStatuses.UNREAD,
            SentAt = DateTime.UtcNow
        }).ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        _dbContext.Notifications.AddRange(notifications);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateForTimeOffReviewedAsync(int timeOffId, string status)
    {
        var timeOff = await _dbContext.TimeOffs
            .AsNoTracking()
            .Where(currentTimeOff => currentTimeOff.Id == timeOffId)
            .Select(currentTimeOff => new
            {
                currentTimeOff.Id,
                currentTimeOff.Employee.UserId
            })
            .FirstOrDefaultAsync();
        if (timeOff is null)
        {
            throw new BadRequestException("Time off request does not exist.");
        }

        var normalizedStatus = status.Trim().ToUpperInvariant();
        var type = normalizedStatus == TimeOffStatuses.APPROVED
            ? NotificationTypes.TIME_OFF_APPROVED
            : NotificationTypes.TIME_OFF_REJECTED;
        var message = normalizedStatus == TimeOffStatuses.APPROVED
            ? "Zahtjev za odsustvo je odobren."
            : "Zahtjev za odsustvo je odbijen.";

        _dbContext.Notifications.Add(new Notification
        {
            UserId = timeOff.UserId,
            TimeOffId = timeOff.Id,
            Type = type,
            Message = message,
            Status = NotificationStatuses.UNREAD,
            SentAt = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(NotificationSearchObject search)
    {
        var currentUserId = GetRequiredCurrentUserId();

        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == currentUserId);

        query = AddFilter(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<NotificationDto?> MarkAsReadAsync(int id)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(currentNotification => currentNotification.Id == id);
        if (notification is null)
        {
            return null;
        }

        EnsureCurrentUserOwnsNotification(notification);

        notification.Status = NotificationStatuses.READ;
        await _dbContext.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task<int> MarkAllAsReadAsync()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var notifications = await _dbContext.Notifications
            .Where(notification =>
                notification.UserId == currentUserId
                && notification.Status == NotificationStatuses.UNREAD)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.Status = NotificationStatuses.READ;
        }

        await _dbContext.SaveChangesAsync();

        return notifications.Count;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(currentNotification => currentNotification.Id == id);
        if (notification is null)
        {
            return false;
        }

        EnsureCurrentUserOwnsNotification(notification);

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteReadAsync()
    {
        var currentUserId = GetRequiredCurrentUserId();

        var notifications = await _dbContext.Notifications
            .Where(notification =>
                notification.UserId == currentUserId
                && notification.Status == NotificationStatuses.READ)
            .ToListAsync();

        _dbContext.Notifications.RemoveRange(notifications);
        await _dbContext.SaveChangesAsync();

        return notifications.Count;
    }

    private async Task<AppointmentNotificationContext> BuildAppointmentContextAsync(int appointmentId, int triggeredByUserId)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(currentAppointment => currentAppointment.Id == appointmentId)
            .Select(currentAppointment => new
            {
                currentAppointment.Id,
                ClientUserId = currentAppointment.Client.UserId,
                ClientEmail = currentAppointment.Client.User.Email,
                EmployeeUserId = currentAppointment.Employee.UserId,
                currentAppointment.Employee.ShopId
            })
            .FirstOrDefaultAsync();
        if (appointment is null)
        {
            throw new BadRequestException("Appointment does not exist.");
        }

        var ownerUserIds = await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole =>
                userRole.ShopId == appointment.ShopId
                && userRole.Role.Name.ToUpper() == RoleNames.OWNER)
            .Select(userRole => userRole.UserId)
            .Distinct()
            .ToListAsync();

        return new AppointmentNotificationContext
        {
            AppointmentId = appointment.Id,
            ClientUserId = appointment.ClientUserId,
            ClientEmail = appointment.ClientEmail,
            EmployeeUserId = appointment.EmployeeUserId,
            TriggeredByUserId = triggeredByUserId,
            OwnerUserIds = ownerUserIds
        };
    }

    private static IQueryable<Notification> AddFilter(IQueryable<Notification> query, NotificationSearchObject search)
    {
        if (!string.IsNullOrWhiteSpace(search.Status))
        {
            var status = search.Status.Trim().ToUpper();
            query = query.Where(notification => notification.Status.ToUpper() == status);
        }

        if (!string.IsNullOrWhiteSpace(search.Type))
        {
            var type = search.Type.Trim().ToUpper();
            query = query.Where(notification => notification.Type.ToUpper() == type);
        }

        if (search.DateFrom.HasValue)
        {
            var dateFrom = ToUtcDateTime(search.DateFrom.Value);
            query = query.Where(notification => notification.SentAt >= dateFrom);
        }

        if (search.DateTo.HasValue)
        {
            var dateTo = ToUtcDateTime(search.DateTo.Value);
            query = query.Where(notification => notification.SentAt <= dateTo);
        }

        return query.OrderByDescending(notification => notification.SentAt);
    }

    private static async Task<PagedResult<NotificationDto>> ToPagedResultAsync(
        IQueryable<Notification> query,
        NotificationSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);

        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var notifications = await query.ToListAsync();

        return new PagedResult<NotificationDto>
        {
            Items = notifications.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    private void EnsureCurrentUserOwnsNotification(Notification notification)
    {
        var currentUserId = GetRequiredCurrentUserId();
        if (notification.UserId != currentUserId)
        {
            throw new ForbiddenException("You do not have access to this notification.");
        }
    }

    private int GetRequiredCurrentUserId()
    {
        return _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            AppointmentId = notification.AppointmentId,
            TimeOffId = notification.TimeOffId,
            Type = notification.Type,
            Title = GetNotificationTitle(notification.Type),
            Message = notification.Message,
            Status = notification.Status,
            SentAt = notification.SentAt
        };
    }

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

    private static string GetNotificationTitle(string type)
    {
        return type switch
        {
            NotificationTypes.APPOINTMENT_BOOKED => "Novi zahtjev za termin",
            NotificationTypes.APPOINTMENT_CONFIRMED => "Termin je potvrđen",
            NotificationTypes.APPOINTMENT_CANCELLED => "Termin je odbijen",
            NotificationTypes.APPOINTMENT_COMPLETED => "Termin je završen",
            NotificationTypes.APPOINTMENT_NO_SHOW => "Termin nije realizovan",
            NotificationTypes.TIME_OFF_REQUESTED => "Novi zahtjev za odsustvo",
            NotificationTypes.TIME_OFF_APPROVED => "Odsustvo je odobreno",
            NotificationTypes.TIME_OFF_REJECTED => "Odsustvo je odbijeno",
            _ => "Notifikacija"
        };
    }
}
