namespace BarberShop.API.Entities;

public class ServiceCategory
{
    public int Id { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool Active { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}
