namespace BarberShop.API.DTOs.Owner;

public class OwnerEmployeeDetailsDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public bool Active { get; set; }
    public IReadOnlyCollection<OwnerEmployeeWorkingHourDto> WorkingHours { get; set; } = [];
    public IReadOnlyCollection<OwnerEmployeeTimeOffDto> TimeOff { get; set; } = [];
    public IReadOnlyCollection<OwnerEmployeeRecentAppointmentDto> RecentAppointments { get; set; } = [];
}

public class OwnerEmployeeWorkingHourDto
{
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public bool Active { get; set; }
}

public class OwnerEmployeeTimeOffDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class OwnerEmployeeRecentAppointmentDto
{
    public int AppointmentId { get; set; }
    public DateTime StartTime { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class OwnerReviewSummaryDto
{
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRatingThisWeek { get; set; }
    public int NewReviewsThisWeek { get; set; }
    public string? TopRatedEmployeeName { get; set; }
}

public class OwnerAnalyticsDto
{
    public int TotalAppointmentsCount { get; set; }
    public int TodayAppointmentsCount { get; set; }
    public int PendingAppointmentsCount { get; set; }
    public int ConfirmedAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int CancelledAppointmentsCount { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int ActiveEmployeesCount { get; set; }
    public int ActiveServicesCount { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal ReturningClientsRate { get; set; }
    public string? MostPopularService { get; set; }
    public int MostPopularServiceAppointmentsThisMonth { get; set; }
    public IReadOnlyCollection<OwnerAnalyticsServiceDto> TopServices { get; set; } = [];
    public IReadOnlyCollection<OwnerAnalyticsEmployeeDto> MostActiveEmployees { get; set; } = [];
}

public class OwnerAnalyticsServiceDto
{
    public int ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AppointmentsCount { get; set; }
}

public class OwnerAnalyticsEmployeeDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int AppointmentsCount { get; set; }
}

public class OwnerShopSettingsDto
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Description { get; set; }
}
