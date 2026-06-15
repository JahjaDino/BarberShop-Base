using System.ComponentModel.DataAnnotations;
using BarberShop.API.Data;
using BarberShop.API.DTOs.Shops;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.SearchObjects.Shops;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Shops;

public class ShopService : BaseCRUDService<Shop, ShopDto, ShopSearchObject, ShopInsertRequest, ShopUpdateRequest>, IShopService
{
    private readonly IOwnerAccessService _ownerAccessService;

    public ShopService(BarberShopDbContext dbContext, IOwnerAccessService ownerAccessService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
    }

    protected override IQueryable<Shop> AddFilter(IQueryable<Shop> query, ShopSearchObject search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.Trim().ToLower();
            query = query.Where(shop => shop.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(search.City))
        {
            var city = search.City.Trim().ToLower();
            query = query.Where(shop => shop.City.ToLower().Contains(city));
        }

        return query;
    }

    protected override ShopDto MapToDto(Shop entity)
    {
        return new ShopDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            City = entity.City,
            Phone = entity.Phone,
            Email = entity.Email,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }

    protected override Shop MapInsertToEntity(ShopInsertRequest request)
    {
        return new Shop
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            City = request.City.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim().ToLower(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    protected override void MapUpdateToEntity(Shop entity, ShopUpdateRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Address = request.Address.Trim();
        entity.City = request.City.Trim();
        entity.Phone = request.Phone.Trim();
        entity.Email = request.Email.Trim().ToLower();
        entity.Description = request.Description?.Trim();
    }

    protected override async Task BeforeInsert(Shop entity, ShopInsertRequest request)
    {
        ValidateRequiredFields(request.Name, request.Email);

        await EnsureNameIsUniqueAsync(request.Name);
        await EnsureEmailIsUniqueAsync(request.Email);
    }

    protected override async Task BeforeUpdate(Shop entity, ShopUpdateRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(entity.Id);

        ValidateRequiredFields(request.Name, request.Email);

        await EnsureNameIsUniqueAsync(request.Name, entity.Id);
        await EnsureEmailIsUniqueAsync(request.Email, entity.Id);
    }

    protected override async Task BeforeDelete(Shop entity)
    {
        await EnsureOwnerCanAccessShopAsync(entity.Id);

        var hasRelatedData =
            await DbContext.ServiceCategories.AnyAsync(category => category.ShopId == entity.Id)
            || await DbContext.Services.AnyAsync(service => service.ShopId == entity.Id)
            || await DbContext.Employees.AnyAsync(employee => employee.ShopId == entity.Id)
            || await DbContext.InventoryItems.AnyAsync(item => item.ShopId == entity.Id)
            || await DbContext.UserRoles.AnyAsync(userRole => userRole.ShopId == entity.Id);

        if (hasRelatedData)
        {
            throw new ConflictException("Shop cannot be deleted because it has related data.");
        }
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("You do not have access to this shop.");
        }
    }

    private async Task EnsureNameIsUniqueAsync(string name, int? currentShopId = null)
    {
        var normalizedName = name.Trim().ToLower();
        var exists = await DbContext.Shops.AnyAsync(shop =>
            shop.Name.ToLower() == normalizedName && (!currentShopId.HasValue || shop.Id != currentShopId.Value));

        if (exists)
        {
            throw new ConflictException("Shop with the same name already exists.");
        }
    }

    private async Task EnsureEmailIsUniqueAsync(string email, int? currentShopId = null)
    {
        var normalizedEmail = email.Trim().ToLower();
        var exists = await DbContext.Shops.AnyAsync(shop =>
            shop.Email.ToLower() == normalizedEmail && (!currentShopId.HasValue || shop.Id != currentShopId.Value));

        if (exists)
        {
            throw new ConflictException("Shop with the same email already exists.");
        }
    }

    private static void ValidateRequiredFields(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Email is required.");
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            throw new BadRequestException("Email is not valid.");
        }
    }
}
