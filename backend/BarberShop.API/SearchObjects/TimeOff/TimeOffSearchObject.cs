namespace BarberShop.API.SearchObjects.TimeOff;

public class TimeOffSearchObject : BaseSearchObject
{
    public int? EmployeeId { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? DateFrom { get; set; }

    public DateTimeOffset? DateTo { get; set; }
}
