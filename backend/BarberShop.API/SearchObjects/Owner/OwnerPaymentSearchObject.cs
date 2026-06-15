using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Owner;

public class OwnerPaymentSearchObject : BaseSearchObject
{
    public DateOnly? Date { get; set; }

    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }
}
