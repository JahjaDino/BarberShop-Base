namespace BarberShop.API.DTOs.Client;

public class ClientAppointmentCardDto
{
    public int AppointmentId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public string? PaymentMethod { get; set; }

    public string Status { get; set; } = string.Empty;
}
