namespace BarberShop.API.DTOs.Appointments;

public class AppointmentBookRequest
{
    public int EmployeeId { get; set; }

    public List<int>? ServiceIds { get; set; } = [];

    public DateTimeOffset StartTime { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }
}
