namespace BarberShop.API.DTOs.Public;

public class PublicShopDto
{
    public int ShopId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Address { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
