using BarberShop.API.Constants;
using BarberShop.API.Controllers.Base;
using BarberShop.API.DTOs.WorkingHours;
using BarberShop.API.SearchObjects.WorkingHours;
using BarberShop.API.Services.WorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize(Roles = RoleNames.OWNER)]
[Route("api/working-hours")]
public class WorkingHoursController
    : BaseCRUDController<WorkingHourDto, WorkingHourSearchObject, WorkingHourInsertRequest, WorkingHourUpdateRequest>
{
    public WorkingHoursController(IWorkingHourService service)
        : base(service)
    {
    }

    [HttpGet("/api/employees/{employeeId:int}/working-hours")]
    public async Task<IActionResult> GetByEmployee(int employeeId, [FromQuery] WorkingHourSearchObject search)
    {
        search.EmployeeId = employeeId;
        return Ok(await Service.GetAsync(search));
    }

    [HttpPost]
    public override Task<IActionResult> Create([FromBody] WorkingHourInsertRequest request)
    {
        return base.Create(request);
    }

    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] WorkingHourUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [HttpDelete("{id:int}")]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
