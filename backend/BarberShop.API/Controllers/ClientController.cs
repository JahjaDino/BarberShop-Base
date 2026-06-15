using BarberShop.API.Constants;
using BarberShop.API.Services.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize(Roles = RoleNames.CLIENT)]
[ApiController]
[Route("api/client")]
public class ClientController : ControllerBase
{
    private readonly IClientPortalService _clientPortalService;

    public ClientController(IClientPortalService clientPortalService)
    {
        _clientPortalService = clientPortalService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int? shopId)
    {
        return Ok(await _clientPortalService.GetDashboardAsync(shopId));
    }

    [HttpGet("favorite-services")]
    public async Task<IActionResult> GetFavoriteServices([FromQuery] int? shopId)
    {
        return Ok(await _clientPortalService.GetFavoriteServicesAsync(shopId));
    }

    [HttpPost("favorite-services/{serviceId:int}")]
    public async Task<IActionResult> AddFavoriteService(int serviceId, [FromQuery] int? shopId)
    {
        return Ok(await _clientPortalService.AddFavoriteServiceAsync(serviceId, shopId));
    }

    [HttpDelete("favorite-services/{serviceId:int}")]
    public async Task<IActionResult> RemoveFavoriteService(int serviceId)
    {
        var removed = await _clientPortalService.RemoveFavoriteServiceAsync(serviceId);

        return removed ? NoContent() : NotFound();
    }
}
