using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Employees;

public class EmployeeUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int ShopId { get; set; }

    [Required]
    public string Position { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public DateOnly EmploymentDate { get; set; }

    public bool Active { get; set; }
}
