namespace BarberShop.API.DTOs.Services;

public class ServiceDto
{
    public int Id { get; set; }

    public int ShopId { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool AllowOverlap { get; set; }

    public int MaxParallelAppointments { get; set; }

    public bool Active { get; set; }
}
