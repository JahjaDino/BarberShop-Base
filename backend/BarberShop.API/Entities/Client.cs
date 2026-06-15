namespace BarberShop.API.Entities;

public class Client
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int LoyaltyPoints { get; set; }

    public string? Notes { get; set; }
}