using BarberShop.API.SearchObjects;
using BarberShop.API.Services.Base;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController<TDto, TSearch> : ControllerBase
    where TSearch : BaseSearchObject
{
    protected readonly IBaseService<TDto, TSearch> Service;

    protected BaseController(IBaseService<TDto, TSearch> service)
    {
        Service = service;
    }

    [HttpGet]
    public virtual async Task<IActionResult> Get([FromQuery] TSearch search)
    {
        return Ok(await Service.GetAsync(search));
    }

    [HttpGet("{id:int}")]
    public virtual async Task<IActionResult> GetById(int id)
    {
        var item = await Service.GetByIdAsync(id);

        return item is null ? NotFound() : Ok(item);
    }
}
