namespace BarberShop.API.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? AppointmentId { get; set; }

    public int? TimeOffId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}
