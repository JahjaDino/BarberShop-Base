namespace BarberShop.API.DTOs.Client;

public class FavoriteServiceDto
{
    public int ServiceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
}
