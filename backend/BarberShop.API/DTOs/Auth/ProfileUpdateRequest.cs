using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Auth;

public class ProfileUpdateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
