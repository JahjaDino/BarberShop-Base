using BarberShop.API.Constants;
using BarberShop.API.Entities;
using BarberShop.API.Services.Email;

namespace BarberShop.API.Services.Notifications.Strategies;

public class AppointmentCancelledNotificationStrategy : INotificationStrategy
{
    public string Type => NotificationTypes.APPOINTMENT_CANCELLED;

    public IReadOnlyCollection<Notification> CreateNotifications(AppointmentNotificationContext context)
    {
        if (context.TriggeredByUserId == context.ClientUserId)
        {
            var notifications = context.OwnerUserIds
                .Distinct()
                .Select(ownerUserId => new Notification
                {
                    UserId = ownerUserId,
                    AppointmentId = context.AppointmentId,
                    Type = Type,
                    Message = "Termin je otkazan.",
                    Status = NotificationStatuses.UNREAD,
                    SentAt = DateTime.UtcNow
                })
                .ToList();

            notifications.Add(new Notification
            {
                UserId = context.EmployeeUserId,
                AppointmentId = context.AppointmentId,
                Type = Type,
                    Message = "Klijent je otkazao termin.",
                Status = NotificationStatuses.UNREAD,
                SentAt = DateTime.UtcNow
            });

            return notifications;
        }

        var clientNotifications = new List<Notification>
        {
            new()
            {
                UserId = context.ClientUserId,
                AppointmentId = context.AppointmentId,
                Type = Type,
                Message = "Termin je odbijen.",
                Status = NotificationStatuses.UNREAD,
                SentAt = DateTime.UtcNow
            }
        };

        if (context.TriggeredByUserId == context.EmployeeUserId)
        {
            clientNotifications.AddRange(context.OwnerUserIds
                .Distinct()
                .Select(ownerUserId => new Notification
                {
                    UserId = ownerUserId,
                    AppointmentId = context.AppointmentId,
                    Type = Type,
                    Message = "Termin je otkazan.",
                    Status = NotificationStatuses.UNREAD,
                    SentAt = DateTime.UtcNow
                }));
        }

        return clientNotifications;
    }

    public IReadOnlyCollection<EmailMessage> CreateEmailMessages(AppointmentNotificationContext context)
    {
        if (context.TriggeredByUserId == context.ClientUserId)
        {
            return [];
        }

        return
        [
            new EmailMessage
            {
                To = context.ClientEmail,
                Subject = "Appointment cancelled",
                Body = "Your appointment has been cancelled."
            }
        ];
    }
}
