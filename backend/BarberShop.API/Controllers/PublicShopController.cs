using BarberShop.API.Services.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/public/shops")]
public class PublicShopController : ControllerBase
{
    private readonly IPublicShopService _publicShopService;

    public PublicShopController(IPublicShopService publicShopService)
    {
        _publicShopService = publicShopService;
    }

    [HttpGet("{shopId:int}")]
    public async Task<IActionResult> GetShop(int shopId)
    {
        var shop = await _publicShopService.GetShopAsync(shopId);

        return shop is null ? NotFound() : Ok(shop);
    }

    [HttpGet("{shopId:int}/service-categories")]
    public async Task<IActionResult> GetServiceCategories(int shopId)
    {
        return Ok(await _publicShopService.GetServiceCategoriesAsync(shopId));
    }

    [HttpGet("{shopId:int}/services")]
    public async Task<IActionResult> GetServices(int shopId)
    {
        return Ok(await _publicShopService.GetServicesAsync(shopId));
    }

    [HttpGet("{shopId:int}/employees")]
    public async Task<IActionResult> GetEmployees(int shopId)
    {
        return Ok(await _publicShopService.GetEmployeesAsync(shopId));
    }

    [HttpGet("{shopId:int}/popular-services")]
    public async Task<IActionResult> GetPopularServices(int shopId)
    {
        return Ok(await _publicShopService.GetPopularServicesAsync(shopId));
    }

    [HttpGet("{shopId:int}/available-slots")]
    public async Task<IActionResult> GetAvailableSlots(
        int shopId,
        [FromQuery] int serviceId,
        [FromQuery] int employeeId,
        [FromQuery] DateOnly date)
    {
        return Ok(await _publicShopService.GetAvailableSlotsAsync(shopId, serviceId, employeeId, date));
    }
}
