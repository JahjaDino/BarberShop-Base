namespace BarberShop.API.SearchObjects.Employees;

public class EmployeeSearchObject : BaseSearchObject
{
    public int? ShopId { get; set; }

    public int? UserId { get; set; }

    public string? Position { get; set; }

    public bool? Active { get; set; }
}
