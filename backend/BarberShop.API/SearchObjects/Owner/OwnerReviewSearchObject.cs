using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Owner;

public class OwnerReviewSearchObject : BaseSearchObject
{
    public int? Rating { get; set; }

    public int? EmployeeId { get; set; }
}
