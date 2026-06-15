namespace BarberShop.API.DTOs.Inventory;

public class InventoryItemDto
{
    public int Id { get; set; }

    public int ShopId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public int MinimumQuantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public bool IsLowStock { get; set; }
}
