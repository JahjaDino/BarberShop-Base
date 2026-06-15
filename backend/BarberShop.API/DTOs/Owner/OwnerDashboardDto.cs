namespace BarberShop.API.DTOs.Owner;

public class OwnerDashboardDto
{
    public OwnerDashboardSummaryDto Summary { get; set; } = new();

    public IReadOnlyCollection<OwnerTodayAppointmentDto> TodayAppointments { get; set; } = [];

    public IReadOnlyCollection<OwnerEmployeeTodayDto> EmployeesToday { get; set; } = [];

    public OwnerBusinessOverviewDto BusinessOverview { get; set; } = new();
}

public class OwnerDashboardSummaryDto
{
    public int TodayAppointmentsCount { get; set; }
    public int ConfirmedTodayAppointmentsCount { get; set; }
    public int ActiveEmployeesCount { get; set; }
    public int AvailableEmployeesCount { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal AverageWeeklyRating { get; set; }
}

public class OwnerTodayAppointmentDto
{
    public int AppointmentId { get; set; }
    public string Time { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class OwnerEmployeeTodayDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int AppointmentsCountToday { get; set; }
    public string AvailabilityStatus { get; set; } = string.Empty;
}

public class OwnerBusinessOverviewDto
{
    public int LowStockItemsCount { get; set; }
    public int PendingPaymentsCount { get; set; }
}
