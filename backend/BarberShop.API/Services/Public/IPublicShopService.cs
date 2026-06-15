using BarberShop.API.DTOs.Appointments;
using BarberShop.API.DTOs.Client;
using BarberShop.API.DTOs.Public;

namespace BarberShop.API.Services.Public;

public interface IPublicShopService
{
    Task<PublicShopDto?> GetShopAsync(int shopId);

    Task<IReadOnlyCollection<PublicServiceCategoryDto>> GetServiceCategoriesAsync(int shopId);

    Task<IReadOnlyCollection<ClientServiceCardDto>> GetServicesAsync(int shopId);

    Task<IReadOnlyCollection<PublicEmployeeDto>> GetEmployeesAsync(int shopId);

    Task<IReadOnlyCollection<ClientServiceCardDto>> GetPopularServicesAsync(int shopId);

    Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsAsync(int shopId, int serviceId, int employeeId, DateOnly date);
}
