using BarberShop.API.Constants;
using BarberShop.API.Entities;
using BarberShop.API.Services.Email;

namespace BarberShop.API.Services.Notifications.Strategies;

public class AppointmentNoShowNotificationStrategy : INotificationStrategy
{
    public string Type => NotificationTypes.APPOINTMENT_NO_SHOW;

    public IReadOnlyCollection<Notification> CreateNotifications(AppointmentNotificationContext context)
    {
        return
        [
            new Notification
            {
                UserId = context.ClientUserId,
                AppointmentId = context.AppointmentId,
                Type = Type,
                Message = "Termin je označen kao propušten.",
                Status = NotificationStatuses.UNREAD,
                SentAt = DateTime.UtcNow
            }
        ];
    }

    public IReadOnlyCollection<EmailMessage> CreateEmailMessages(AppointmentNotificationContext context)
    {
        return [];
    }
}
