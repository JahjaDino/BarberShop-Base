namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeProfileDto
{
    public int EmployeeId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string ShopName { get; set; } = string.Empty;

    public IReadOnlyCollection<EmployeeWorkingHoursSummaryDto> WorkingHoursSummary { get; set; } = [];
}

public class EmployeeWorkingHoursSummaryDto
{
    public int DayOfWeek { get; set; }

    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;

    public bool Active { get; set; }
}
