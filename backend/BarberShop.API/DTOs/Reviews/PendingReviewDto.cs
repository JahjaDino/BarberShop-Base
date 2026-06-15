namespace BarberShop.API.DTOs.Reviews;

public class PendingReviewDto
{
    public int AppointmentId { get; set; }

    public string ServiceName { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }
}
