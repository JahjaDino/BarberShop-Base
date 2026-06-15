using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeTimeOffCreateRequest
{
    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}
