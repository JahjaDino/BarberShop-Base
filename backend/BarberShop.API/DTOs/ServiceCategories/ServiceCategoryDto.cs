namespace BarberShop.API.DTOs.ServiceCategories;

public class ServiceCategoryDto
{
    public int Id { get; set; }

    public int ShopId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool Active { get; set; }
}
