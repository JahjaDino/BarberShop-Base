using BarberShop.API.Constants;
using BarberShop.API.Controllers.Base;
using BarberShop.API.DTOs.Services;
using BarberShop.API.SearchObjects.Services;
using BarberShop.API.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Route("api/services")]
public class ServicesController : BaseCRUDController<ServiceDto, ServiceSearchObject, ServiceInsertRequest, ServiceUpdateRequest>
{
    public ServicesController(IServiceService service)
        : base(service)
    {
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] ServiceInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] ServiceUpdateRequest request)
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
