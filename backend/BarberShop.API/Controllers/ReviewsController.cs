using BarberShop.API.Constants;
using BarberShop.API.DTOs.Reviews;
using BarberShop.API.SearchObjects.Reviews;
using BarberShop.API.Services.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReviewInsertRequest request)
    {
        var review = await _reviewService.CreateAsync(request);

        return StatusCode(StatusCodes.Status201Created, review);
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpGet("my")]
    public async Task<IActionResult> GetMine([FromQuery] ReviewSearchObject search)
    {
        return Ok(await _reviewService.GetMineAsync(search));
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        return Ok(await _reviewService.GetPendingAsync());
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] ReviewSearchObject search)
    {
        return Ok(await _reviewService.GetAsync(search));
    }

    [Authorize(Roles = $"{RoleNames.CLIENT},{RoleNames.OWNER}")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);

        return review is null ? NotFound() : Ok(review);
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _reviewService.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}
