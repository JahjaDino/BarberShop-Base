using BarberShop.API.Constants;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Client;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Client;

public class ClientPortalService : IClientPortalService
{
    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public ClientPortalService(BarberShopDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyCollection<FavoriteServiceDto>> GetFavoriteServicesAsync(int? shopId)
    {
        var clientId = await GetCurrentClientIdAsync();

        var query = _dbContext.ClientFavoriteServices
            .AsNoTracking()
            .Where(favorite => favorite.ClientId == clientId);

        if (shopId.HasValue)
        {
            await EnsureShopExistsAsync(shopId.Value);
            query = query.Where(favorite => favorite.Service.ShopId == shopId.Value);
        }

        return await query
            .OrderByDescending(favorite => favorite.CreatedAt)
            .Select(favorite => new FavoriteServiceDto
            {
                ServiceId = favorite.ServiceId,
                Name = favorite.Service.Name,
                Description = favorite.Service.Description,
                CategoryName = favorite.Service.Category.Name,
                DurationMinutes = favorite.Service.DurationMinutes,
                Price = favorite.Service.Price,
                CreatedAt = favorite.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<FavoriteServiceDto> AddFavoriteServiceAsync(int serviceId, int? shopId)
    {
        var clientId = await GetCurrentClientIdAsync();

        var service = await _dbContext.Services
            .AsNoTracking()
            .Where(currentService => currentService.Id == serviceId)
            .Select(currentService => new
            {
                currentService.Id,
                currentService.ShopId,
                currentService.Active
            })
            .FirstOrDefaultAsync();
        if (service is null)
        {
            throw new NotFoundException("Service does not exist.");
        }

        if (!service.Active)
        {
            throw new BadRequestException("Service is not active.");
        }

        if (shopId.HasValue)
        {
            await EnsureShopExistsAsync(shopId.Value);
            if (service.ShopId != shopId.Value)
            {
                throw new BadRequestException("Service does not belong to the selected shop.");
            }
        }

        var alreadyFavorite = await _dbContext.ClientFavoriteServices
            .AnyAsync(favorite => favorite.ClientId == clientId && favorite.ServiceId == serviceId);
        if (alreadyFavorite)
        {
            throw new ConflictException("Service is already in favorites.");
        }

        _dbContext.ClientFavoriteServices.Add(new ClientFavoriteService
        {
            ClientId = clientId,
            ServiceId = serviceId,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();

        return await _dbContext.ClientFavoriteServices
            .AsNoTracking()
            .Where(favorite => favorite.ClientId == clientId && favorite.ServiceId == serviceId)
            .Select(favorite => new FavoriteServiceDto
            {
                ServiceId = favorite.ServiceId,
                Name = favorite.Service.Name,
                Description = favorite.Service.Description,
                CategoryName = favorite.Service.Category.Name,
                DurationMinutes = favorite.Service.DurationMinutes,
                Price = favorite.Service.Price,
                CreatedAt = favorite.CreatedAt
            })
            .FirstAsync();
    }

    public async Task<bool> RemoveFavoriteServiceAsync(int serviceId)
    {
        var clientId = await GetCurrentClientIdAsync();

        var favorite = await _dbContext.ClientFavoriteServices
            .FirstOrDefaultAsync(currentFavorite =>
                currentFavorite.ClientId == clientId && currentFavorite.ServiceId == serviceId);
        if (favorite is null)
        {
            return false;
        }

        _dbContext.ClientFavoriteServices.Remove(favorite);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<ClientDashboardDto> GetDashboardAsync(int? shopId)
    {
        var clientId = await GetCurrentClientIdAsync();
        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var now = DateTime.UtcNow;

        if (shopId.HasValue)
        {
            await EnsureShopExistsAsync(shopId.Value);
        }

        var nextAppointmentQuery = _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.ClientId == clientId
                && AppointmentStatuses.Blocking.Contains(appointment.Status)
                && appointment.StartTime > now);

        if (shopId.HasValue)
        {
            nextAppointmentQuery = nextAppointmentQuery
                .Where(appointment => appointment.Employee.ShopId == shopId.Value);
        }

        var nextAppointment = await nextAppointmentQuery
            .OrderBy(appointment => appointment.StartTime)
            .Select(appointment => new ClientAppointmentCardDto
            {
                AppointmentId = appointment.Id,
                ServiceName = appointment.AppointmentServices
                    .OrderBy(appointmentService => appointmentService.Id)
                    .Select(appointmentService => appointmentService.Service.Name)
                    .FirstOrDefault() ?? string.Empty,
                EmployeeName = appointment.Employee.User.FirstName + " " + appointment.Employee.User.LastName,
                Date = DateOnly.FromDateTime(appointment.StartTime),
                Time = TimeOnly.FromDateTime(appointment.StartTime),
                DurationMinutes = appointment.AppointmentServices.Sum(appointmentService => appointmentService.DurationAtBooking),
                Price = appointment.TotalPrice,
                PaymentMethod = _dbContext.Payments
                    .Where(payment => payment.AppointmentId == appointment.Id)
                    .OrderByDescending(payment => payment.Id)
                    .Select(payment => payment.PaymentMethod)
                    .FirstOrDefault(),
                Status = appointment.Status
            })
            .FirstOrDefaultAsync();

        var effectiveShopId = shopId ?? await ResolveClientShopIdAsync(clientId, now);
        var popularServices = effectiveShopId.HasValue
            ? await GetPopularServicesAsync(effectiveShopId.Value)
            : Array.Empty<ClientServiceCardDto>();

        var unreadNotificationsCount = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(notification => notification.UserId == currentUserId
                && notification.Status == NotificationStatuses.UNREAD);

        return new ClientDashboardDto
        {
            NextAppointment = nextAppointment,
            PopularServices = popularServices,
            UnreadNotificationsCount = unreadNotificationsCount
        };
    }

    private async Task EnsureShopExistsAsync(int shopId)
    {
        var exists = await _dbContext.Shops
            .AsNoTracking()
            .AnyAsync(shop => shop.Id == shopId);
        if (!exists)
        {
            throw new NotFoundException("Shop does not exist.");
        }
    }

    private async Task<int?> ResolveClientShopIdAsync(int clientId, DateTime now)
    {
        var nextShopId = await _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.ClientId == clientId
                && AppointmentStatuses.Blocking.Contains(appointment.Status)
                && appointment.StartTime > now)
            .OrderBy(appointment => appointment.StartTime)
            .Select(appointment => (int?)appointment.Employee.ShopId)
            .FirstOrDefaultAsync();
        if (nextShopId.HasValue)
        {
            return nextShopId.Value;
        }

        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.ClientId == clientId)
            .OrderByDescending(appointment => appointment.StartTime)
            .Select(appointment => (int?)appointment.Employee.ShopId)
            .FirstOrDefaultAsync();
    }

    private async Task<IReadOnlyCollection<ClientServiceCardDto>> GetPopularServicesAsync(int shopId)
    {
        var popularServices = await _dbContext.AppointmentServices
            .AsNoTracking()
            .Where(appointmentService => appointmentService.Service.ShopId == shopId)
            .GroupBy(appointmentService => new
            {
                appointmentService.ServiceId,
                appointmentService.Service.Name,
                appointmentService.Service.Description,
                CategoryName = appointmentService.Service.Category.Name,
                appointmentService.Service.DurationMinutes,
                appointmentService.Service.Price,
                appointmentService.Service.Active
            })
            .Where(group => group.Key.Active)
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
            .Where(service => service.ShopId == shopId && service.Active && !existingIds.Contains(service.Id))
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

    private async Task<int> GetCurrentClientIdAsync()
    {
        var currentUserId = _currentUserService.CurrentUserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var clientId = await _dbContext.Clients
            .AsNoTracking()
            .Where(client => client.UserId == currentUserId)
            .Select(client => (int?)client.Id)
            .FirstOrDefaultAsync();

        if (!clientId.HasValue)
        {
            throw new ForbiddenException("Current user does not have a client profile.");
        }

        return clientId.Value;
    }
}
