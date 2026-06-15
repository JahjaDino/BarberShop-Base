namespace BarberShop.API.SearchObjects.WorkingHours;

public class WorkingHourSearchObject : BaseSearchObject
{
    public int? EmployeeId { get; set; }

    public int? DayOfWeek { get; set; }

    public bool? Active { get; set; }
}
