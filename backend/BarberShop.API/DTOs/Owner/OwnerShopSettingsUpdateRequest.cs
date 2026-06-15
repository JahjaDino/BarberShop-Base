using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Owner;

public class OwnerShopSettingsUpdateRequest
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(250)]
    public string Address { get; set; } = string.Empty;

    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }
}
