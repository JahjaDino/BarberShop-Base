using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.ServiceCategories;

public class ServiceCategoryInsertRequest
{
    [Range(1, int.MaxValue)]
    public int ShopId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
