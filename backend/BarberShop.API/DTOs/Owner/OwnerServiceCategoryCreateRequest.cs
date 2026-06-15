using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Owner;

public class OwnerServiceCategoryCreateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
