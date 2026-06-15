using BarberShop.API.Constants;
using BarberShop.API.Entities;
using BarberShop.API.Services.Email;

namespace BarberShop.API.Services.Notifications.Strategies;

public class AppointmentBookedNotificationStrategy : INotificationStrategy
{
    public string Type => NotificationTypes.APPOINTMENT_BOOKED;

    public IReadOnlyCollection<Notification> CreateNotifications(AppointmentNotificationContext context)
    {
        var notifications = new List<Notification>
        {
            CreateNotification(
                context,
                context.ClientUserId,
                "Vaš termin je uspješno poslan i čeka potvrdu.")
        };

        notifications.AddRange(context.OwnerUserIds
            .Distinct()
            .Select(ownerUserId => CreateNotification(
                context,
                ownerUserId,
                "Novi termin je rezervisan.")));

        notifications.Add(CreateNotification(
            context,
            context.EmployeeUserId,
            "Novi zahtjev za termin čeka vašu potvrdu."));

        return notifications;
    }

    public IReadOnlyCollection<EmailMessage> CreateEmailMessages(AppointmentNotificationContext context)
    {
        return
        [
            new EmailMessage
            {
                To = context.ClientEmail,
                Subject = "Appointment request received",
                Body = "Your appointment request has been submitted and is waiting for confirmation."
            }
        ];
    }

    private static Notification CreateNotification(AppointmentNotificationContext context, int userId, string message)
    {
        return new Notification
        {
            UserId = userId,
            AppointmentId = context.AppointmentId,
            Type = NotificationTypes.APPOINTMENT_BOOKED,
            Message = message,
            Status = NotificationStatuses.UNREAD,
            SentAt = DateTime.UtcNow
        };
    }
}
