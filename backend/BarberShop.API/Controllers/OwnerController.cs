using BarberShop.API.Constants;
using BarberShop.API.DTOs.Owner;
using BarberShop.API.SearchObjects.Inventory;
using BarberShop.API.SearchObjects.Owner;
using BarberShop.API.Services.Owner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize(Roles = RoleNames.OWNER)]
[ApiController]
[Route("api/owner")]
public class OwnerController : ControllerBase
{
    private readonly IOwnerPortalService _ownerPortalService;

    public OwnerController(IOwnerPortalService ownerPortalService)
    {
        _ownerPortalService = ownerPortalService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard() => Ok(await _ownerPortalService.GetDashboardAsync());

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] OwnerAppointmentSearchObject search)
        => Ok(await _ownerPortalService.GetAppointmentsAsync(search));

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees() => Ok(await _ownerPortalService.GetEmployeesAsync());

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee([FromBody] OwnerEmployeeCreateRequest request)
        => StatusCode(StatusCodes.Status201Created, await _ownerPortalService.CreateEmployeeAsync(request));

    [HttpPatch("employees/{id:int}/activate")]
    public async Task<IActionResult> ActivateEmployee(int id)
        => Ok(await _ownerPortalService.ActivateEmployeeAsync(id));

    [HttpPatch("employees/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateEmployee(int id)
        => Ok(await _ownerPortalService.DeactivateEmployeeAsync(id));

    [HttpGet("employees/{id:int}")]
    public async Task<IActionResult> GetEmployeeDetails(int id)
    {
        var employee = await _ownerPortalService.GetEmployeeDetailsAsync(id);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpGet("service-categories")]
    public async Task<IActionResult> GetServiceCategories()
        => Ok(await _ownerPortalService.GetServiceCategoriesAsync());

    [HttpPost("service-categories")]
    public async Task<IActionResult> CreateServiceCategory([FromBody] OwnerServiceCategoryCreateRequest request)
        => StatusCode(StatusCodes.Status201Created, await _ownerPortalService.CreateServiceCategoryAsync(request));

    [HttpPut("service-categories/{id:int}")]
    public async Task<IActionResult> UpdateServiceCategory(int id, [FromBody] OwnerServiceCategoryUpdateRequest request)
        => Ok(await _ownerPortalService.UpdateServiceCategoryAsync(id, request));

    [HttpGet("services")]
    public async Task<IActionResult> GetServices([FromQuery] OwnerServiceSearchObject search)
        => Ok(await _ownerPortalService.GetServicesAsync(search));

    [HttpPost("services")]
    public async Task<IActionResult> CreateService([FromBody] OwnerServiceCreateRequest request)
        => StatusCode(StatusCodes.Status201Created, await _ownerPortalService.CreateServiceAsync(request));

    [HttpPut("services/{id:int}")]
    public async Task<IActionResult> UpdateService(int id, [FromBody] OwnerServiceUpdateRequest request)
        => Ok(await _ownerPortalService.UpdateServiceAsync(id, request));

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] OwnerReviewSearchObject search)
        => Ok(await _ownerPortalService.GetReviewsAsync(search));

    [HttpGet("reviews/summary")]
    public async Task<IActionResult> GetReviewSummary() => Ok(await _ownerPortalService.GetReviewSummaryAsync());

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory([FromQuery] InventoryItemSearchObject search)
        => Ok(await _ownerPortalService.GetInventoryAsync(search));

    [HttpGet("inventory/low-stock")]
    public async Task<IActionResult> GetLowStockInventory() => Ok(await _ownerPortalService.GetLowStockInventoryAsync());

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] OwnerPaymentSearchObject search)
        => Ok(await _ownerPortalService.GetPaymentsAsync(search));

    [HttpGet("time-off-requests")]
    public async Task<IActionResult> GetTimeOffRequests([FromQuery] OwnerTimeOffRequestSearchObject search)
        => Ok(await _ownerPortalService.GetTimeOffRequestsAsync(search));

    [HttpPatch("time-off-requests/{id:int}/approve")]
    public async Task<IActionResult> ApproveTimeOffRequest(int id, [FromBody] OwnerTimeOffReviewRequest request)
        => Ok(await _ownerPortalService.ApproveTimeOffRequestAsync(id, request));

    [HttpPatch("time-off-requests/{id:int}/reject")]
    public async Task<IActionResult> RejectTimeOffRequest(int id, [FromBody] OwnerTimeOffReviewRequest request)
        => Ok(await _ownerPortalService.RejectTimeOffRequestAsync(id, request));

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics() => Ok(await _ownerPortalService.GetAnalyticsAsync());

    [HttpGet("shop-settings")]
    public async Task<IActionResult> GetShopSettings() => Ok(await _ownerPortalService.GetShopSettingsAsync());

    [HttpPatch("shop-settings")]
    public async Task<IActionResult> UpdateShopSettings([FromBody] OwnerShopSettingsUpdateRequest request)
        => Ok(await _ownerPortalService.UpdateShopSettingsAsync(request));
}
