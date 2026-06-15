namespace BarberShop.API.Entities;

public class AppointmentService
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public decimal PriceAtBooking { get; set; }

    public int DurationAtBooking { get; set; }
}
