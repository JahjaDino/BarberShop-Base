namespace BarberShop.API.DTOs.Public;

public class PublicServiceCategoryDto
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
