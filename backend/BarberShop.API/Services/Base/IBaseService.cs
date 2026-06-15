using BarberShop.API.Models;
using BarberShop.API.SearchObjects;

namespace BarberShop.API.Services.Base;

public interface IBaseService<TDto, TSearch>
    where TSearch : BaseSearchObject
{
    Task<PagedResult<TDto>> GetAsync(TSearch search);

    Task<TDto?> GetByIdAsync(int id);
}
