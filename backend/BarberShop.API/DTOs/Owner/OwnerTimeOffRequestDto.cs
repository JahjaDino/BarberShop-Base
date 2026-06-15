namespace BarberShop.API.DTOs.Owner;

public class OwnerTimeOffRequestDto
{
    public int TimeOffId { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }
}
