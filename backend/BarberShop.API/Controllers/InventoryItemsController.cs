using BarberShop.API.Constants;
using BarberShop.API.DTOs.Inventory;
using BarberShop.API.SearchObjects.Inventory;
using BarberShop.API.Services.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize(Roles = RoleNames.OWNER)]
[ApiController]
[Route("api/inventory-items")]
public class InventoryItemsController : ControllerBase
{
    private readonly IInventoryItemService _inventoryItemService;

    public InventoryItemsController(IInventoryItemService inventoryItemService)
    {
        _inventoryItemService = inventoryItemService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] InventoryItemSearchObject search)
    {
        return Ok(await _inventoryItemService.GetAsync(search));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] InventoryItemSearchObject search)
    {
        return Ok(await _inventoryItemService.GetLowStockAsync(search));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _inventoryItemService.GetByIdAsync(id);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryItemInsertRequest request)
    {
        var item = await _inventoryItemService.CreateAsync(request);

        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] InventoryItemUpdateRequest request)
    {
        var item = await _inventoryItemService.UpdateAsync(id, request);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _inventoryItemService.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}
