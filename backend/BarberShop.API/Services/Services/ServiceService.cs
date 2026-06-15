using BarberShop.API.Data;
using BarberShop.API.DTOs.Services;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.Services;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;
using ServiceEntity = BarberShop.API.Entities.Service;

namespace BarberShop.API.Services.Services;

public class ServiceService
    : BaseCRUDService<ServiceEntity, ServiceDto, ServiceSearchObject, ServiceInsertRequest, ServiceUpdateRequest>,
        IServiceService
{
    private readonly IOwnerAccessService _ownerAccessService;

    public ServiceService(BarberShopDbContext dbContext, IOwnerAccessService ownerAccessService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
    }

    public override async Task<PagedResult<ServiceDto>> GetAsync(ServiceSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);

        var query = DbContext.Services.AsNoTracking().AsQueryable();
        query = AddFilter(query, search);
        query = AddOrder(query, search);

        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var items = await query
            .Select(service => new ServiceDto
            {
                Id = service.Id,
                ShopId = service.ShopId,
                CategoryId = service.CategoryId,
                CategoryName = service.Category.Name,
                Name = service.Name,
                Description = service.Description,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                Active = service.Active
            })
            .ToListAsync();

        return new PagedResult<ServiceDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    public override async Task<ServiceDto?> GetByIdAsync(int id)
    {
        return await DbContext.Services
            .AsNoTracking()
            .Where(service => service.Id == id)
            .Select(service => new ServiceDto
            {
                Id = service.Id,
                ShopId = service.ShopId,
                CategoryId = service.CategoryId,
                CategoryName = service.Category.Name,
                Name = service.Name,
                Description = service.Description,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                Active = service.Active
            })
            .FirstOrDefaultAsync();
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.Services.FirstOrDefaultAsync(service => service.Id == id);

        if (entity is null)
        {
            throw new NotFoundException("Service was not found.");
        }

        await BeforeDelete(entity);

        entity.Active = false;
        await DbContext.SaveChangesAsync();

        await AfterDelete(entity);

        return true;
    }

    protected override IQueryable<ServiceEntity> AddInclude(IQueryable<ServiceEntity> query, ServiceSearchObject? search)
    {
        return query;
    }

    protected override IQueryable<ServiceEntity> AddFilter(IQueryable<ServiceEntity> query, ServiceSearchObject search)
    {
        if (search.ShopId.HasValue)
        {
            query = query.Where(service => service.ShopId == search.ShopId.Value);
        }

        if (search.CategoryId.HasValue)
        {
            query = query.Where(service => service.CategoryId == search.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.Trim().ToLower();
            query = query.Where(service => service.Name.ToLower().Contains(name));
        }

        if (search.Active.HasValue)
        {
            query = query.Where(service => service.Active == search.Active.Value);
        }

        if (search.MinPrice.HasValue)
        {
            query = query.Where(service => service.Price >= search.MinPrice.Value);
        }

        if (search.MaxPrice.HasValue)
        {
            query = query.Where(service => service.Price <= search.MaxPrice.Value);
        }

        return query;
    }

    protected override ServiceDto MapToDto(ServiceEntity entity)
    {
        return new ServiceDto
        {
            Id = entity.Id,
            ShopId = entity.ShopId,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name ?? string.Empty,
            Name = entity.Name,
            Description = entity.Description,
            DurationMinutes = entity.DurationMinutes,
            Price = entity.Price,
            Active = entity.Active
        };
    }

    protected override ServiceEntity MapInsertToEntity(ServiceInsertRequest request)
    {
        return new ServiceEntity
        {
            ShopId = request.ShopId,
            CategoryId = request.CategoryId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price,
            Active = true
        };
    }

    protected override void MapUpdateToEntity(ServiceEntity entity, ServiceUpdateRequest request)
    {
        entity.ShopId = request.ShopId;
        entity.CategoryId = request.CategoryId;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.DurationMinutes = request.DurationMinutes;
        entity.Price = request.Price;
        entity.Active = request.Active;
    }

    protected override async Task BeforeInsert(ServiceEntity entity, ServiceInsertRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(request.ShopId);
        await ValidateServiceRequestAsync(request.ShopId, request.CategoryId, request.Name, request.DurationMinutes, request.Price);
        await EnsureNameIsUniqueAsync(request.ShopId, request.Name);
    }

    protected override async Task BeforeUpdate(ServiceEntity entity, ServiceUpdateRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);
        await EnsureOwnerCanAccessShopAsync(request.ShopId);
        await ValidateServiceRequestAsync(request.ShopId, request.CategoryId, request.Name, request.DurationMinutes, request.Price);
        await EnsureNameIsUniqueAsync(request.ShopId, request.Name, entity.Id);
    }

    protected override async Task BeforeDelete(ServiceEntity entity)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);
    }

    private async Task ValidateServiceRequestAsync(
        int shopId,
        int categoryId,
        string name,
        int durationMinutes,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (durationMinutes <= 0)
        {
            throw new BadRequestException("Duration must be greater than 0 minutes.");
        }

        if (durationMinutes > 480)
        {
            throw new BadRequestException("Duration cannot be greater than 480 minutes.");
        }

        if (price < 0)
        {
            throw new BadRequestException("Price cannot be negative.");
        }

        if (price > 1000)
        {
            throw new BadRequestException("Price cannot be greater than 1000.");
        }

        var shopExists = await DbContext.Shops.AnyAsync(shop => shop.Id == shopId);
        if (!shopExists)
        {
            throw new BadRequestException("Shop does not exist.");
        }

        var category = await DbContext.ServiceCategories
            .AsNoTracking()
            .Where(currentCategory => currentCategory.Id == categoryId)
            .Select(currentCategory => new
            {
                currentCategory.ShopId,
                currentCategory.Active
            })
            .FirstOrDefaultAsync();
        if (category is null)
        {
            throw new BadRequestException("Service category does not exist.");
        }

        if (category.ShopId != shopId)
        {
            throw new BadRequestException("Service category does not belong to the selected shop.");
        }

        if (!category.Active)
        {
            throw new BadRequestException("Service category is not active.");
        }
    }

    private async Task EnsureNameIsUniqueAsync(int shopId, string name, int? currentServiceId = null)
    {
        var normalizedName = name.Trim().ToLower();
        var exists = await DbContext.Services.AnyAsync(service =>
            service.ShopId == shopId
            && service.Name.ToLower() == normalizedName
            && (!currentServiceId.HasValue || service.Id != currentServiceId.Value));

        if (exists)
        {
            throw new ConflictException("Service with the same name already exists in this shop.");
        }
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("You do not have access to this shop.");
        }
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return BaseSearchObject.DefaultPageSize;
        }

        return Math.Min(pageSize, BaseSearchObject.MaxPageSize);
    }
}
