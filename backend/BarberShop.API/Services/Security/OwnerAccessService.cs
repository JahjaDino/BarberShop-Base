using BarberShop.API.Constants;
using BarberShop.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Security;

public class OwnerAccessService : IOwnerAccessService
{
    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public OwnerAccessService(BarberShopDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<int?> GetOwnerShopIdAsync()
    {
        var currentUserId = _currentUserService.CurrentUserId;
        if (!currentUserId.HasValue)
        {
            return null;
        }

        return await _dbContext.UserRoles
            .AsNoTracking()
            .Where(userRole =>
                userRole.UserId == currentUserId.Value
                && userRole.ShopId.HasValue
                && userRole.Role.Name.ToUpper() == RoleNames.OWNER)
            .Select(userRole => userRole.ShopId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CanAccessShopAsync(int shopId)
    {
        var ownerShopId = await GetOwnerShopIdAsync();

        return ownerShopId.HasValue && ownerShopId.Value == shopId;
    }
}
