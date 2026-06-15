using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Owner;

public class OwnerServiceCategoryUpdateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool Active { get; set; }
}
