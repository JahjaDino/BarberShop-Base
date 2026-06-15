namespace BarberShop.API.DTOs.Employees;

public class EmployeeDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ShopId { get; set; }

    public string Position { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public DateOnly EmploymentDate { get; set; }

    public bool Active { get; set; }
}
