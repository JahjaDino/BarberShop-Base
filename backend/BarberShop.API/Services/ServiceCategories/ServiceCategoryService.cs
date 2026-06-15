using BarberShop.API.Data;
using BarberShop.API.DTOs.ServiceCategories;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.SearchObjects.ServiceCategories;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.ServiceCategories;

public class ServiceCategoryService
    : BaseCRUDService<ServiceCategory, ServiceCategoryDto, ServiceCategorySearchObject, ServiceCategoryInsertRequest, ServiceCategoryUpdateRequest>,
        IServiceCategoryService
{
    private readonly IOwnerAccessService _ownerAccessService;

    public ServiceCategoryService(BarberShopDbContext dbContext, IOwnerAccessService ownerAccessService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.ServiceCategories.FirstOrDefaultAsync(category => category.Id == id);

        if (entity is null)
        {
            throw new NotFoundException("Service category was not found.");
        }

        await BeforeDelete(entity);

        entity.Active = false;
        await DbContext.SaveChangesAsync();

        await AfterDelete(entity);

        return true;
    }

    protected override IQueryable<ServiceCategory> AddFilter(
        IQueryable<ServiceCategory> query,
        ServiceCategorySearchObject search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.Trim().ToLower();
            query = query.Where(category => category.Name.ToLower().Contains(name));
        }

        if (search.Active.HasValue)
        {
            query = query.Where(category => category.Active == search.Active.Value);
        }

        return query;
    }

    protected override ServiceCategoryDto MapToDto(ServiceCategory entity)
    {
        return new ServiceCategoryDto
        {
            Id = entity.Id,
            ShopId = entity.ShopId,
            Name = entity.Name,
            Description = entity.Description,
            Active = entity.Active
        };
    }

    protected override ServiceCategory MapInsertToEntity(ServiceCategoryInsertRequest request)
    {
        return new ServiceCategory
        {
            ShopId = request.ShopId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Active = true
        };
    }

    protected override void MapUpdateToEntity(ServiceCategory entity, ServiceCategoryUpdateRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.Active = request.Active;
    }

    protected override async Task BeforeInsert(ServiceCategory entity, ServiceCategoryInsertRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(request.ShopId);

        ValidateName(request.Name);

        var nameExists = await DbContext.ServiceCategories.AnyAsync(
            category => category.Name.ToLower() == request.Name.Trim().ToLower());

        if (nameExists)
        {
            throw new ConflictException("Service category with the same name already exists.");
        }
    }

    protected override async Task BeforeUpdate(ServiceCategory entity, ServiceCategoryUpdateRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);

        ValidateName(request.Name);

        var nameExists = await DbContext.ServiceCategories.AnyAsync(
            category => category.Id != entity.Id && category.Name.ToLower() == request.Name.Trim().ToLower());

        if (nameExists)
        {
            throw new ConflictException("Service category with the same name already exists.");
        }
    }

    protected override async Task BeforeDelete(ServiceCategory entity)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("You do not have access to this shop.");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }
    }
}
