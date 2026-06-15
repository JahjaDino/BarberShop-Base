namespace BarberShop.API.Services.Security;

public interface IOwnerAccessService
{
    Task<int?> GetOwnerShopIdAsync();

    Task<bool> CanAccessShopAsync(int shopId);
}
