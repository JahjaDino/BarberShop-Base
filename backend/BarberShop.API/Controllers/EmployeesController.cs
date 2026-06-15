using BarberShop.API.Constants;
using BarberShop.API.Controllers.Base;
using BarberShop.API.DTOs.Employees;
using BarberShop.API.SearchObjects.Employees;
using BarberShop.API.Services.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Route("api/employees")]
public class EmployeesController : BaseCRUDController<EmployeeDto, EmployeeSearchObject, EmployeeInsertRequest, EmployeeUpdateRequest>
{
    public EmployeesController(IEmployeeService service)
        : base(service)
    {
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] EmployeeInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateRequest request)
    {
        return base.Update(id, request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpDelete("{id:int}")]
    public override Task<IActionResult> Delete(int id)
    {
        return base.Delete(id);
    }
}
