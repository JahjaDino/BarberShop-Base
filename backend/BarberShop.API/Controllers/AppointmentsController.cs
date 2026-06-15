using BarberShop.API.Constants;
using BarberShop.API.DTOs.Appointments;
using BarberShop.API.SearchObjects.Appointments;
using BarberShop.API.Services.Appointments;
using BarberShop.API.Services.Appointments.Booking;
using BarberShop.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentBookingFacade _bookingFacade;
    private readonly IAppointmentManagementService _managementService;

    public AppointmentsController(
        IAppointmentBookingFacade bookingFacade,
        IAppointmentManagementService managementService)
    {
        _bookingFacade = bookingFacade;
        _managementService = managementService;
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpPost("book")]
    [EnableRateLimiting(AuthRateLimitPolicies.AppointmentBooking)]
    public async Task<IActionResult> Book([FromBody] AppointmentBookRequest request)
    {
        var appointment = await _bookingFacade.BookAsync(request);

        return StatusCode(StatusCodes.Status201Created, appointment);
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpGet("my")]
    public async Task<IActionResult> GetMine([FromQuery] ClientAppointmentSearchObject search)
    {
        if (!string.IsNullOrWhiteSpace(search.Filter))
        {
            return Ok(await _managementService.GetMyClientCardsAsync(search));
        }

        return Ok(await _managementService.GetMineAsync(new AppointmentSearchObject
        {
            Page = search.Page,
            PageSize = search.PageSize,
            IncludeTotalCount = search.IncludeTotalCount,
            GetAll = search.GetAll
        }));
    }

    [Authorize(Roles = RoleNames.CLIENT)]
    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] int serviceId, [FromQuery] int employeeId, [FromQuery] DateOnly date)
    {
        return Ok(await _managementService.GetAvailableSlotsAsync(serviceId, employeeId, date));
    }

    [Authorize(Roles = RoleNames.EMPLOYEE)]
    [HttpGet("employee/my")]
    public async Task<IActionResult> GetMyEmployeeAppointments([FromQuery] AppointmentSearchObject search)
    {
        return Ok(await _managementService.GetMyEmployeeAppointmentsAsync(search));
    }

    [Authorize(Roles = $"{RoleNames.CLIENT},{RoleNames.OWNER},{RoleNames.EMPLOYEE}")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _managementService.GetByIdAsync(id);

        return appointment is null ? NotFound() : Ok(appointment);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AppointmentSearchObject search)
    {
        return Ok(await _managementService.GetAsync(search));
    }

    [Authorize(Roles = $"{RoleNames.CLIENT},{RoleNames.OWNER},{RoleNames.EMPLOYEE}")]
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] AppointmentCancelRequest? request)
    {
        return Ok(await _managementService.CancelAsync(id, request ?? new AppointmentCancelRequest()));
    }

    [Authorize(Roles = $"{RoleNames.OWNER},{RoleNames.EMPLOYEE}")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatusUpdateRequest request)
    {
        var appointment = await _managementService.UpdateStatusAsync(id, request);

        return appointment is null ? NotFound() : Ok(appointment);
    }
}
