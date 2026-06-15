using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.Owner;

public class OwnerTimeOffReviewRequest
{
    [MaxLength(500)]
    public string? ReviewNote { get; set; }
}
