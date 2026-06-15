using BarberShop.API.DTOs.Client;

namespace BarberShop.API.Services.Client;

public interface IClientPortalService
{
    Task<IReadOnlyCollection<FavoriteServiceDto>> GetFavoriteServicesAsync(int? shopId);

    Task<FavoriteServiceDto> AddFavoriteServiceAsync(int serviceId, int? shopId);

    Task<bool> RemoveFavoriteServiceAsync(int serviceId);

    Task<ClientDashboardDto> GetDashboardAsync(int? shopId);
}
