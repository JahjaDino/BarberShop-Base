namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeDashboardDto
{
    public EmployeeDashboardSummaryDto Summary { get; set; } = new();

    public IReadOnlyCollection<EmployeeScheduleItemDto> TodaySchedule { get; set; } = [];

    public IReadOnlyCollection<EmployeeAssignedAppointmentDto> AssignedAppointments { get; set; } = [];

    public EmployeeTimeOffSummaryDto TimeOffSummary { get; set; } = new();
}

public class EmployeeDashboardSummaryDto
{
    public int TodayAppointmentsCount { get; set; }

    public int ConfirmedTodayAppointmentsCount { get; set; }

    public DateTime? NextAppointmentTime { get; set; }

    public string? NextAppointmentServiceName { get; set; }

    public IReadOnlyCollection<EmployeeScheduleWorkingHoursDto> WorkingHoursToday { get; set; } = [];

    public string DayStatus { get; set; } = string.Empty;
}

public class EmployeeAssignedAppointmentDto
{
    public int AppointmentId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string Time { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}

public class EmployeeTimeOffSummaryDto
{
    public bool HasActiveTimeOff { get; set; }

    public int ActiveTimeOffCount { get; set; }

    public int PendingTimeOffCount { get; set; }

    public DateTime? NextTimeOffDate { get; set; }
}
