using BarberShop.API.DTOs.Notifications;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects.Notifications;

namespace BarberShop.API.Services.Notifications;

public interface INotificationService
{
    Task CreateForAppointmentEventAsync(int appointmentId, string type, int triggeredByUserId);

    Task CreateForTimeOffRequestedAsync(int timeOffId);

    Task CreateForTimeOffReviewedAsync(int timeOffId, string status);

    Task<PagedResult<NotificationDto>> GetMyNotificationsAsync(NotificationSearchObject search);

    Task<NotificationDto?> MarkAsReadAsync(int id);

    Task<int> MarkAllAsReadAsync();

    Task<bool> DeleteAsync(int id);

    Task<int> DeleteReadAsync();
}
