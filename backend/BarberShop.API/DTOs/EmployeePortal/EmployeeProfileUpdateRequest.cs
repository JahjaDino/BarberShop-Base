using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeProfileUpdateRequest
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
