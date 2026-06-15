namespace BarberShop.API.DTOs.Appointments;

public class AppointmentServiceDto
{
    public int ServiceId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public decimal PriceAtBooking { get; set; }

    public int DurationAtBooking { get; set; }
}
