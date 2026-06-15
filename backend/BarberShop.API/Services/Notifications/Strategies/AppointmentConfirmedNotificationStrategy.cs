using BarberShop.API.Constants;
using BarberShop.API.Entities;
using BarberShop.API.Services.Email;

namespace BarberShop.API.Services.Notifications.Strategies;

public class AppointmentConfirmedNotificationStrategy : INotificationStrategy
{
    public string Type => NotificationTypes.APPOINTMENT_CONFIRMED;

    public IReadOnlyCollection<Notification> CreateNotifications(AppointmentNotificationContext context)
    {
        return
        [
            new Notification
            {
                UserId = context.ClientUserId,
                AppointmentId = context.AppointmentId,
                Type = Type,
                Message = "Termin je potvrđen.",
                Status = NotificationStatuses.UNREAD,
                SentAt = DateTime.UtcNow
            }
        ];
    }

    public IReadOnlyCollection<EmailMessage> CreateEmailMessages(AppointmentNotificationContext context)
    {
        return
        [
            new EmailMessage
            {
                To = context.ClientEmail,
                Subject = "Appointment confirmed",
                Body = "Your appointment has been confirmed."
            }
        ];
    }
}
