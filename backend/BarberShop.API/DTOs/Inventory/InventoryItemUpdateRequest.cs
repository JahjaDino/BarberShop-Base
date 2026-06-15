namespace BarberShop.API.DTOs.Inventory;

public class InventoryItemUpdateRequest
{
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public int MinimumQuantity { get; set; }
}
