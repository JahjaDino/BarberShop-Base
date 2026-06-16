using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Owner;

public class OwnerServiceUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DurationMinutes { get; set; }

    public decimal Price { get; set; }

    public bool AllowOverlap { get; set; }

    public int MaxParallelAppointments { get; set; } = 1;

    public bool Active { get; set; }
}
