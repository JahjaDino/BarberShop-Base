using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Inventory;

public class InventoryItemSearchObject : BaseSearchObject
{
    public string? Name { get; set; }

    public string? Unit { get; set; }

    public bool? LowStockOnly { get; set; }
}
