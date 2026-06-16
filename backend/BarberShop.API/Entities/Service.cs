namespace BarberShop.API.Entities;

public class Service
{
    public int Id { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public int CategoryId { get; set; }
    public ServiceCategory Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool AllowOverlap { get; set; }

    public int MaxParallelAppointments { get; set; } = 1;

    public bool Active { get; set; }
}
