using BarberShop.API.Data;
using BarberShop.API.DTOs.Inventory;
using BarberShop.API.Entities;
using BarberShop.API.Exceptions;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using BarberShop.API.SearchObjects.Inventory;
using BarberShop.API.Services.Base;
using BarberShop.API.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Inventory;

public class InventoryItemService
    : BaseCRUDService<InventoryItem, InventoryItemDto, InventoryItemSearchObject, InventoryItemInsertRequest, InventoryItemUpdateRequest>,
        IInventoryItemService
{
    private const int MaxNameLength = 150;
    private const int MaxUnitLength = 50;

    private readonly IOwnerAccessService _ownerAccessService;

    public InventoryItemService(BarberShopDbContext dbContext, IOwnerAccessService ownerAccessService)
        : base(dbContext)
    {
        _ownerAccessService = ownerAccessService;
    }

    public override async Task<PagedResult<InventoryItemDto>> GetAsync(InventoryItemSearchObject search)
    {
        var ownerShopId = await GetRequiredOwnerShopIdAsync();

        var query = DbContext.InventoryItems
            .AsNoTracking()
            .Where(item => item.ShopId == ownerShopId);

        query = AddFilter(query, search);
        query = AddOrder(query, search);

        return await ToPagedResultAsync(query, search);
    }

    public async Task<PagedResult<InventoryItemDto>> GetLowStockAsync(InventoryItemSearchObject search)
    {
        search.LowStockOnly = true;

        return await GetAsync(search);
    }

    public override async Task<InventoryItemDto?> GetByIdAsync(int id)
    {
        var ownerShopId = await GetRequiredOwnerShopIdAsync();

        return await DbContext.InventoryItems
            .AsNoTracking()
            .Where(item => item.Id == id && item.ShopId == ownerShopId)
            .Select(item => MapToDto(item))
            .FirstOrDefaultAsync();
    }

    public override async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.InventoryItems.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null)
        {
            throw new NotFoundException("Inventory item does not exist.");
        }

        await BeforeDelete(entity);

        DbContext.InventoryItems.Remove(entity);
        await DbContext.SaveChangesAsync();

        return true;
    }

    protected override IQueryable<InventoryItem> AddFilter(IQueryable<InventoryItem> query, InventoryItemSearchObject search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.Trim().ToLower();
            query = query.Where(item => item.Name.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(search.Unit))
        {
            var unit = search.Unit.Trim().ToLower();
            query = query.Where(item => item.Unit.ToLower().Contains(unit));
        }

        if (search.LowStockOnly == true)
        {
            query = query.Where(item => item.Quantity <= item.MinimumQuantity);
        }

        return query;
    }

    protected override InventoryItemDto MapToDto(InventoryItem entity)
    {
        return new InventoryItemDto
        {
            Id = entity.Id,
            ShopId = entity.ShopId,
            Name = entity.Name,
            Quantity = entity.Quantity,
            Unit = entity.Unit,
            MinimumQuantity = entity.MinimumQuantity,
            LastUpdated = entity.LastUpdated,
            IsLowStock = entity.Quantity <= entity.MinimumQuantity
        };
    }

    protected override InventoryItem MapInsertToEntity(InventoryItemInsertRequest request)
    {
        return new InventoryItem
        {
            Name = request.Name.Trim(),
            Quantity = request.Quantity,
            Unit = request.Unit.Trim(),
            MinimumQuantity = request.MinimumQuantity,
            LastUpdated = DateTime.UtcNow
        };
    }

    protected override void MapUpdateToEntity(InventoryItem entity, InventoryItemUpdateRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Quantity = request.Quantity;
        entity.Unit = request.Unit.Trim();
        entity.MinimumQuantity = request.MinimumQuantity;
        entity.LastUpdated = DateTime.UtcNow;
    }

    protected override async Task BeforeInsert(InventoryItem entity, InventoryItemInsertRequest request)
    {
        var ownerShopId = await GetRequiredOwnerShopIdAsync();

        ValidateRequest(request.Name, request.Quantity, request.Unit, request.MinimumQuantity);
        await EnsureNameIsUniqueAsync(ownerShopId, request.Name);

        entity.ShopId = ownerShopId;
    }

    protected override async Task BeforeUpdate(InventoryItem entity, InventoryItemUpdateRequest request)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);

        ValidateRequest(request.Name, request.Quantity, request.Unit, request.MinimumQuantity);
        await EnsureNameIsUniqueAsync(entity.ShopId, request.Name, entity.Id);
    }

    protected override async Task BeforeDelete(InventoryItem entity)
    {
        await EnsureOwnerCanAccessShopAsync(entity.ShopId);
    }

    private async Task<PagedResult<InventoryItemDto>> ToPagedResultAsync(
        IQueryable<InventoryItem> query,
        InventoryItemSearchObject search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);

        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        query = search.GetAll
            ? query.Take(BaseSearchObject.MaxPageSize)
            : query.Skip(page * pageSize).Take(pageSize);

        var items = await query
            .Select(item => new InventoryItemDto
            {
                Id = item.Id,
                ShopId = item.ShopId,
                Name = item.Name,
                Quantity = item.Quantity,
                Unit = item.Unit,
                MinimumQuantity = item.MinimumQuantity,
                LastUpdated = item.LastUpdated,
                IsLowStock = item.Quantity <= item.MinimumQuantity
            })
            .ToListAsync();

        return new PagedResult<InventoryItemDto>
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

    private async Task EnsureNameIsUniqueAsync(int shopId, string name, int? currentItemId = null)
    {
        var normalizedName = name.Trim().ToLower();
        var exists = await DbContext.InventoryItems.AnyAsync(item =>
            item.ShopId == shopId
            && item.Name.ToLower() == normalizedName
            && (!currentItemId.HasValue || item.Id != currentItemId.Value));

        if (exists)
        {
            throw new ConflictException("Inventory item with the same name already exists in this shop.");
        }
    }

    private async Task<int> GetRequiredOwnerShopIdAsync()
    {
        var ownerShopId = await _ownerAccessService.GetOwnerShopIdAsync();
        if (!ownerShopId.HasValue)
        {
            throw new ForbiddenException("You do not have access to inventory items.");
        }

        return ownerShopId.Value;
    }

    private async Task EnsureOwnerCanAccessShopAsync(int shopId)
    {
        if (!await _ownerAccessService.CanAccessShopAsync(shopId))
        {
            throw new ForbiddenException("You do not have access to this inventory item.");
        }
    }

    private static void ValidateRequest(string name, int quantity, string unit, int minimumQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (name.Trim().Length > MaxNameLength)
        {
            throw new BadRequestException($"Name cannot be longer than {MaxNameLength} characters.");
        }

        if (quantity < 0)
        {
            throw new BadRequestException("Quantity cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(unit))
        {
            throw new BadRequestException("Unit is required.");
        }

        if (unit.Trim().Length > MaxUnitLength)
        {
            throw new BadRequestException($"Unit cannot be longer than {MaxUnitLength} characters.");
        }

        if (minimumQuantity < 0)
        {
            throw new BadRequestException("Minimum quantity cannot be negative.");
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
