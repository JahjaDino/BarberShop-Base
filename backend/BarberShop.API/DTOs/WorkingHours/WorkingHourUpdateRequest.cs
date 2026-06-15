using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.WorkingHours;

public class WorkingHourUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    public int? DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool Active { get; set; }
}
