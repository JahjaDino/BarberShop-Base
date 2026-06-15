using BarberShop.API.Constants;
using BarberShop.API.Controllers.Base;
using BarberShop.API.DTOs.TimeOff;
using BarberShop.API.SearchObjects.TimeOff;
using BarberShop.API.Services.TimeOff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Route("api/time-off")]
public class TimeOffController : BaseCRUDController<TimeOffDto, TimeOffSearchObject, TimeOffInsertRequest, TimeOffUpdateRequest>
{
    private readonly ITimeOffService _timeOffService;

    public TimeOffController(ITimeOffService service)
        : base(service)
    {
        _timeOffService = service;
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpGet]
    public override Task<IActionResult> Get([FromQuery] TimeOffSearchObject search)
    {
        return base.Get(search);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpGet("{id:int}")]
    public override Task<IActionResult> GetById(int id)
    {
        return base.GetById(id);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpGet("/api/employees/{employeeId:int}/time-off")]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] TimeOffSearchObject search)
    {
        search.EmployeeId = employeeId;
        return Ok(await Service.GetAsync(search));
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] TimeOffInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] TimeOffUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpDelete("{id:int}")]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TimeOffStatusUpdateRequest request)
    {
        var item = await _timeOffService.UpdateStatusAsync(id, request);

        return item is null ? NotFound() : Ok(item);
    }
}
