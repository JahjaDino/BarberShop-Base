namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeScheduleDto
{
    public DateOnly Date { get; set; }

    public IReadOnlyCollection<EmployeeScheduleWorkingHoursDto> WorkingHours { get; set; } = [];

    public IReadOnlyCollection<EmployeeScheduleItemDto> Items { get; set; } = [];
}

public class EmployeeScheduleWorkingHoursDto
{
    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;
}

public class EmployeeScheduleItemDto
{
    public string Time { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ClientName { get; set; }

    public int? AppointmentId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Status { get; set; }
}
