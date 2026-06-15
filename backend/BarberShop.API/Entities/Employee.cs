namespace BarberShop.API.Entities;

public class Employee
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public string Position { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public DateOnly EmploymentDate { get; set; }

    public bool Active { get; set; }
}