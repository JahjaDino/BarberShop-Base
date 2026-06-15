namespace BarberShop.API.Entities;

public class ClientFavoriteService
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
