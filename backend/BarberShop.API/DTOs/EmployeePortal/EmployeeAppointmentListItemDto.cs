namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeAppointmentListItemDto
{
    public int AppointmentId { get; set; }

    public DateOnly Date { get; set; }

    public string Time { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;
}
