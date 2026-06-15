using BarberShop.API.Entities;

namespace BarberShop.API.Entities;
public class UserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public int? ShopId { get; set; }
    public Shop? Shop { get; set; }
}
