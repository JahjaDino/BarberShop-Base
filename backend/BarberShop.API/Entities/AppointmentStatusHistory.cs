namespace BarberShop.API.Entities;

public class AppointmentStatusHistory
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public string? OldStatus { get; set; }

    public string NewStatus { get; set; } = string.Empty;

    public int ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public string? Reason { get; set; }
}
