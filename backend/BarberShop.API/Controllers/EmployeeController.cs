using BarberShop.API.Constants;
using BarberShop.API.DTOs.EmployeePortal;
using BarberShop.API.SearchObjects.EmployeePortal;
using BarberShop.API.Services.EmployeePortal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize(Roles = RoleNames.EMPLOYEE)]
[ApiController]
[Route("api/employee")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeePortalService _employeePortalService;

    public EmployeeController(IEmployeePortalService employeePortalService)
    {
        _employeePortalService = employeePortalService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateOnly? date)
        => Ok(await _employeePortalService.GetDashboardAsync(date));

    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule([FromQuery] DateOnly? date)
        => Ok(await _employeePortalService.GetScheduleAsync(date));

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] EmployeeAppointmentSearchObject search)
        => Ok(await _employeePortalService.GetAppointmentsAsync(search));

    [HttpGet("time-off")]
    public async Task<IActionResult> GetTimeOff()
        => Ok(await _employeePortalService.GetTimeOffAsync());

    [HttpPost("time-off")]
    public async Task<IActionResult> CreateTimeOff([FromBody] EmployeeTimeOffCreateRequest request)
        => StatusCode(StatusCodes.Status201Created, await _employeePortalService.CreateTimeOffAsync(request));

    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
        => Ok(await _employeePortalService.GetServicesAsync());

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory()
        => Ok(await _employeePortalService.GetInventoryAsync());

    [HttpPost("inventory")]
    public async Task<IActionResult> ReportInventory([FromBody] EmployeeInventoryReportRequest request)
        => StatusCode(StatusCodes.Status201Created, await _employeePortalService.ReportInventoryAsync(request));

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
        => Ok(await _employeePortalService.GetProfileAsync());

    [HttpPatch("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] EmployeeProfileUpdateRequest request)
        => Ok(await _employeePortalService.UpdateProfileAsync(request));
}
