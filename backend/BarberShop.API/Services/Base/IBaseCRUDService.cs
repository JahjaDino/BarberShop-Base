using BarberShop.API.SearchObjects;

namespace BarberShop.API.Services.Base;

public interface IBaseCRUDService<TDto, TSearch, TInsert, TUpdate> : IBaseService<TDto, TSearch>
    where TSearch : BaseSearchObject
{
    Task<TDto> CreateAsync(TInsert request);

    Task<TDto?> UpdateAsync(int id, TUpdate request);

    Task<bool> DeleteAsync(int id);
}
