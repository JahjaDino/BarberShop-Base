using BarberShop.API.SearchObjects;
using BarberShop.API.Services.Base;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers.Base;

public abstract class BaseCRUDController<TDto, TSearch, TInsert, TUpdate> : BaseController<TDto, TSearch>
    where TSearch : BaseSearchObject
{
    protected readonly IBaseCRUDService<TDto, TSearch, TInsert, TUpdate> CrudService;

    protected BaseCRUDController(IBaseCRUDService<TDto, TSearch, TInsert, TUpdate> service)
        : base(service)
    {
        CrudService = service;
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TInsert request)
    {
        var item = await CrudService.CreateAsync(request);

        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Update(int id, [FromBody] TUpdate request)
    {
        var item = await CrudService.UpdateAsync(id, request);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var deleted = await CrudService.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}
