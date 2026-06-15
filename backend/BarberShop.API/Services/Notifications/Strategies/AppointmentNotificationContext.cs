namespace BarberShop.API.Services.Notifications.Strategies;

public class AppointmentNotificationContext
{
    public int AppointmentId { get; set; }

    public int ClientUserId { get; set; }

    public string ClientEmail { get; set; } = string.Empty;

    public int EmployeeUserId { get; set; }

    public int TriggeredByUserId { get; set; }

    public List<int> OwnerUserIds { get; set; } = [];
}
