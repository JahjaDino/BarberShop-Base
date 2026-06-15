namespace BarberShop.API.DTOs.Reviews;

public class ReviewDto
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime AppointmentStartTime { get; set; }
}
