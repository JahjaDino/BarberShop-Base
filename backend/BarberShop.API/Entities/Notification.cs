namespace BarberShop.API.Entities;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public int? TimeOffId { get; set; }
    public TimeOff? TimeOff { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }
}
