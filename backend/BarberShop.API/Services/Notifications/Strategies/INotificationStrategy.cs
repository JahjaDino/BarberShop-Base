using BarberShop.API.Entities;
using BarberShop.API.Services.Email;

namespace BarberShop.API.Services.Notifications.Strategies;

public interface INotificationStrategy
{
    string Type { get; }

    IReadOnlyCollection<Notification> CreateNotifications(AppointmentNotificationContext context);

    IReadOnlyCollection<EmailMessage> CreateEmailMessages(AppointmentNotificationContext context);
}
