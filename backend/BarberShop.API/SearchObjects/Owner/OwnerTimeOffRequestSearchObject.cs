using BarberShop.API.DTOs.TimeOff;

namespace BarberShop.API.SearchObjects.Owner;

public class OwnerTimeOffRequestSearchObject : BaseSearchObject
{
    public TimeOffStatus? Status { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }
}
