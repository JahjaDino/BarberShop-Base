using BarberShop.API.Data;
using BarberShop.API.DTOs.Appointments;
using BarberShop.API.DTOs.Client;
using BarberShop.API.DTOs.Public;
using BarberShop.API.Exceptions;
using BarberShop.API.Services.Appointments;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Public;

public class PublicShopService : IPublicShopService
{
    private readonly BarberShopDbContext _dbContext;
    private readonly IAppointmentManagementService _appointmentManagementService;

    public PublicShopService(
        BarberShopDbContext dbContext,
        IAppointmentManagementService appointmentManagementService)
    {
        _dbContext = dbContext;
        _appointmentManagementService = appointmentManagementService;
    }

    public async Task<PublicShopDto?> GetShopAsync(int shopId)
    {
        return await _dbContext.Shops
            .AsNoTracking()
            .Where(shop => shop.Id == shopId)
            .Select(shop => new PublicShopDto
            {
                ShopId = shop.Id,
                Name = shop.Name,
                Description = shop.Description,
                Address = shop.Address,
                Phone = shop.Phone,
                Email = shop.Email
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<PublicServiceCategoryDto>> GetServiceCategoriesAsync(int shopId)
    {
        await EnsureShopExistsAsync(shopId);

        return await _dbContext.ServiceCategories
            .AsNoTracking()
            .Where(category => category.ShopId == shopId
                && category.Active
                && category.Services.Any(service => service.Active))
            .OrderBy(category => category.Name)
            .Select(category => new PublicServiceCategoryDto
            {
                CategoryId = category.Id,
                Name = category.Name,
                Description = category.Description
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<ClientServiceCardDto>> GetServicesAsync(int shopId)
    {
        await EnsureShopExistsAsync(shopId);

        return await _dbContext.Services
            .AsNoTracking()
            .Where(service => service.ShopId == shopId && service.Active)
            .OrderBy(service => service.Category.Name)
            .ThenBy(service => service.Name)
            .Select(service => new ClientServiceCardDto
            {
                ServiceId = service.Id,
                Name = service.Name,
                Description = service.Description,
                CategoryName = service.Category.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<PublicEmployeeDto>> GetEmployeesAsync(int shopId)
    {
        await EnsureShopExistsAsync(shopId);

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.ShopId == shopId && employee.Active)
            .OrderBy(employee => employee.User.FirstName)
            .ThenBy(employee => employee.User.LastName)
            .Select(employee => new PublicEmployeeDto
            {
                EmployeeId = employee.Id,
                FullName = employee.User.FirstName + " " + employee.User.LastName,
                Specialization = employee.Position,
                Rating = _dbContext.Reviews
                    .Where(review => review.Appointment.EmployeeId == employee.Id)
                    .Select(review => (decimal?)review.Rating)
                    .Average()
            })
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<ClientServiceCardDto>> GetPopularServicesAsync(int shopId)
    {
        await EnsureShopExistsAsync(shopId);

        var popularServices = await _dbContext.AppointmentServices
            .AsNoTracking()
            .Where(appointmentService => appointmentService.Service.ShopId == shopId
                && appointmentService.Service.Active)
            .GroupBy(appointmentService => new
            {
                appointmentService.ServiceId,
                appointmentService.Service.Name,
                appointmentService.Service.Description,
                CategoryName = appointmentService.Service.Category.Name,
                appointmentService.Service.DurationMinutes,
                appointmentService.Service.Price
            })
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key.Name)
            .Take(3)
            .Select(group => new ClientServiceCardDto
            {
                ServiceId = group.Key.ServiceId,
                Name = group.Key.Name,
                Description = group.Key.Description,
                CategoryName = group.Key.CategoryName,
                DurationMinutes = group.Key.DurationMinutes,
                Price = group.Key.Price
            })
            .ToListAsync();

        if (popularServices.Count >= 3)
        {
            return popularServices;
        }

        var existingIds = popularServices.Select(service => service.ServiceId).ToHashSet();
        var fallbackServices = await _dbContext.Services
            .AsNoTracking()
            .Where(service => service.ShopId == shopId
                && service.Active
                && !existingIds.Contains(service.Id))
            .OrderBy(service => service.Name)
            .Take(3 - popularServices.Count)
            .Select(service => new ClientServiceCardDto
            {
                ServiceId = service.Id,
                Name = service.Name,
                Description = service.Description,
                CategoryName = service.Category.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price
            })
            .ToListAsync();

        return popularServices.Concat(fallbackServices).ToList();
    }

    public async Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsAsync(
        int shopId,
        int serviceId,
        int employeeId,
        DateOnly date)
    {
        await EnsureShopExistsAsync(shopId);

        return await _appointmentManagementService.GetAvailableSlotsAsync(shopId, serviceId, employeeId, date);
    }

    private async Task EnsureShopExistsAsync(int shopId)
    {
        var shopExists = await _dbContext.Shops
            .AsNoTracking()
            .AnyAsync(shop => shop.Id == shopId);
        if (!shopExists)
        {
            throw new NotFoundException("Shop does not exist.");
        }
    }
}
