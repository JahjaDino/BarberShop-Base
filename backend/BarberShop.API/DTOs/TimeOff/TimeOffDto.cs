namespace BarberShop.API.DTOs.TimeOff;

public class TimeOffDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }

    public string? ReviewedByName { get; set; }
}
