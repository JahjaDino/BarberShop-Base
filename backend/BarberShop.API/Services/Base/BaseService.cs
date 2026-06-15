using BarberShop.API.Data;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Base;

public abstract class BaseService<TEntity, TDto, TSearch> : IBaseService<TDto, TSearch>
    where TEntity : class
    where TSearch : BaseSearchObject
{
    protected readonly BarberShopDbContext DbContext;

    protected BaseService(BarberShopDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual async Task<PagedResult<TDto>> GetAsync(TSearch search)
    {
        var page = Math.Max(0, search.Page);
        var pageSize = NormalizePageSize(search.PageSize);

        var query = DbContext.Set<TEntity>().AsNoTracking().AsQueryable();
        query = AddInclude(query, search);
        query = AddFilter(query, search);
        query = AddOrder(query, search);

        var totalCount = search.IncludeTotalCount
            ? await query.CountAsync()
            : 0;

        if (search.GetAll)
        {
            query = query.Take(BaseSearchObject.MaxPageSize);
        }
        else
        {
            query = query.Skip(page * pageSize).Take(pageSize);
        }

        var entities = await query.ToListAsync();

        return new PagedResult<TDto>
        {
            Items = entities.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = search.IncludeTotalCount
                ? (int)Math.Ceiling(totalCount / (double)pageSize)
                : 0
        };
    }

    public virtual async Task<TDto?> GetByIdAsync(int id)
    {
        var query = DbContext.Set<TEntity>().AsNoTracking().AsQueryable();
        query = AddInclude(query, default);
        query = BeforeGetById(query);

        var entity = await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id);

        return entity is null ? default : MapToDto(entity);
    }

    protected virtual IQueryable<TEntity> AddFilter(IQueryable<TEntity> query, TSearch search)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> AddInclude(IQueryable<TEntity> query, TSearch? search)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> AddOrder(IQueryable<TEntity> query, TSearch search)
    {
        if (string.IsNullOrWhiteSpace(search.OrderBy))
        {
            return query;
        }

        var property = typeof(TEntity).GetProperties()
            .FirstOrDefault(currentProperty =>
                string.Equals(currentProperty.Name, search.OrderBy, StringComparison.OrdinalIgnoreCase));

        if (property is null)
        {
            return query;
        }

        return IsDescending(search.SortDirection)
            ? query.OrderByDescending(entity => EF.Property<object>(entity, property.Name))
            : query.OrderBy(entity => EF.Property<object>(entity, property.Name));
    }

    protected abstract TDto MapToDto(TEntity entity);

    protected virtual IQueryable<TEntity> BeforeGetById(IQueryable<TEntity> query)
    {
        return query;
    }

    private static bool IsDescending(string? sortDirection)
    {
        return string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            || string.Equals(sortDirection, "descending", StringComparison.OrdinalIgnoreCase);
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
