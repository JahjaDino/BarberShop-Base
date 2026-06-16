namespace BarberShop.API.DTOs.Owner;

public class OwnerAppointmentListItemDto
{
    public int AppointmentId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OwnerEmployeeListItemDto
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Active { get; set; }
    public string AvailabilityStatus { get; set; } = string.Empty;
    public int AppointmentsCountToday { get; set; }
    public decimal? AverageRating { get; set; }
}

public class OwnerServiceListItemDto
{
    public int ServiceId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool AllowOverlap { get; set; }
    public int MaxParallelAppointments { get; set; }
    public int BookingsCount { get; set; }
    public bool IsActive { get; set; }
}

public class OwnerReviewListItemDto
{
    public int ReviewId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime AppointmentDate { get; set; }
}

public class OwnerInventoryListItemDto
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinimumQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public string? ReportedByEmployeeName { get; set; }
    public DateTime? ReportedAt { get; set; }
    public string? ReportNote { get; set; }
}

public class OwnerPaymentListItemDto
{
    public int AppointmentId { get; set; }
    public DateOnly Date { get; set; }
    public string Time { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentStatus { get; set; }
    public string AppointmentStatus { get; set; } = string.Empty;
}
