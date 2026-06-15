using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.TimeOff;

public class TimeOffInsertRequest
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }

    public string? Reason { get; set; }
}
