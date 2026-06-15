namespace BarberShop.API.SearchObjects.Services;

public class ServiceSearchObject : BaseSearchObject
{
    public int? ShopId { get; set; }

    public int? CategoryId { get; set; }

    public string? Name { get; set; }

    public bool? Active { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }
}
