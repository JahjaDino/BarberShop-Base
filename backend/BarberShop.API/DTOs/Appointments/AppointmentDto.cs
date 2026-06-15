namespace BarberShop.API.DTOs.Appointments;

public class AppointmentDto
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<AppointmentServiceDto> Services { get; set; } = [];
}
