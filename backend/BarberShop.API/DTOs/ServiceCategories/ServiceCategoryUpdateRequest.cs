using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.ServiceCategories;

public class ServiceCategoryUpdateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool Active { get; set; }
}
