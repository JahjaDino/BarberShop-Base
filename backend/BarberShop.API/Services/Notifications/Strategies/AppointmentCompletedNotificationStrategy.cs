using BarberShop.API.Constants;
using BarberShop.API.Entities;
using BarberShop.API.Services.Email;

namespace BarberShop.API.Services.Notifications.Strategies;

public class AppointmentCompletedNotificationStrategy : INotificationStrategy
{
    public string Type => NotificationTypes.APPOINTMENT_COMPLETED;

    public IReadOnlyCollection<Notification> CreateNotifications(AppointmentNotificationContext context)
    {
        return
        [
            new Notification
            {
                UserId = context.ClientUserId,
                AppointmentId = context.AppointmentId,
                Type = Type,
                Message = "Vaš termin je završen. Možete ostaviti recenziju.",
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
