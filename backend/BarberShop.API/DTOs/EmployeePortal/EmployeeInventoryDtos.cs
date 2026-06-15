using System.ComponentModel.DataAnnotations;

namespace BarberShop.API.DTOs.EmployeePortal;

public class EmployeeInventoryItemDto
{
    public int ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int MinimumQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public string? ReportNote { get; set; }
}

public class EmployeeInventoryReportRequest
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumQuantity { get; set; }

    [StringLength(30)]
    public string? Unit { get; set; }

    [StringLength(30)]
    public string? Status { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
