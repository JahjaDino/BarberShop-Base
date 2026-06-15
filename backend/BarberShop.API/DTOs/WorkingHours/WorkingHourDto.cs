namespace BarberShop.API.DTOs.WorkingHours;

public class WorkingHourDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public bool Active { get; set; }
}
