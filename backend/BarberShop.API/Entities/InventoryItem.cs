namespace BarberShop.API.Entities;

public class InventoryItem
{
    public int Id { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public int MinimumQuantity { get; set; }

    public DateTime LastUpdated { get; set; }

    public int? ReportedByEmployeeId { get; set; }
    public Employee? ReportedByEmployee { get; set; }

    public DateTime? ReportedAt { get; set; }

    public string? ReportNote { get; set; }
}
