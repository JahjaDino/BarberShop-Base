using BarberShop.API.Data;
using BarberShop.API.SearchObjects;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Base;

public abstract class BaseCRUDService<TEntity, TDto, TSearch, TInsert, TUpdate>
    : BaseService<TEntity, TDto, TSearch>, IBaseCRUDService<TDto, TSearch, TInsert, TUpdate>
    where TEntity : class
    where TSearch : BaseSearchObject
{
    protected BaseCRUDService(BarberShopDbContext dbContext)
        : base(dbContext)
    {
    }

    public virtual async Task<TDto> CreateAsync(TInsert request)
    {
        var entity = MapInsertToEntity(request);

        await BeforeInsert(entity, request);

        DbContext.Set<TEntity>().Add(entity);
        await DbContext.SaveChangesAsync();

        await AfterInsert(entity, request);

        return MapToDto(entity);
    }

    public virtual async Task<TDto?> UpdateAsync(int id, TUpdate request)
    {
        var entity = await DbContext.Set<TEntity>()
            .FirstOrDefaultAsync(currentEntity => EF.Property<int>(currentEntity, "Id") == id);

        if (entity is null)
        {
            return default;
        }

        await BeforeUpdate(entity, request);

        MapUpdateToEntity(entity, request);
        await DbContext.SaveChangesAsync();

        await AfterUpdate(entity, request);

        return MapToDto(entity);
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await DbContext.Set<TEntity>()
            .FirstOrDefaultAsync(currentEntity => EF.Property<int>(currentEntity, "Id") == id);

        if (entity is null)
        {
            return false;
        }

        await BeforeDelete(entity);

        DbContext.Set<TEntity>().Remove(entity);
        await DbContext.SaveChangesAsync();

        await AfterDelete(entity);

        return true;
    }

    protected abstract TEntity MapInsertToEntity(TInsert request);

    protected abstract void MapUpdateToEntity(TEntity entity, TUpdate request);

    protected virtual Task BeforeInsert(TEntity entity, TInsert request)
    {
        return Task.CompletedTask;
    }

    protected virtual Task BeforeUpdate(TEntity entity, TUpdate request)
    {
        return Task.CompletedTask;
    }

    protected virtual Task BeforeDelete(TEntity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task AfterInsert(TEntity entity, TInsert request)
    {
        return Task.CompletedTask;
    }

    protected virtual Task AfterUpdate(TEntity entity, TUpdate request)
    {
        return Task.CompletedTask;
    }

    protected virtual Task AfterDelete(TEntity entity)
    {
        return Task.CompletedTask;
    }
}
