namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeTimeOffDto
{
    public int TimeOffId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }

    public string? ReviewedByName { get; set; }
}
