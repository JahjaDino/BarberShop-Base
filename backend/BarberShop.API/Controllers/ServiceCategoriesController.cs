using BarberShop.API.Constants;
using BarberShop.API.Controllers.Base;
using BarberShop.API.DTOs.ServiceCategories;
using BarberShop.API.SearchObjects.ServiceCategories;
using BarberShop.API.Services.ServiceCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Route("api/service-categories")]
public class ServiceCategoriesController
    : BaseCRUDController<ServiceCategoryDto, ServiceCategorySearchObject, ServiceCategoryInsertRequest, ServiceCategoryUpdateRequest>
{
    public ServiceCategoriesController(IServiceCategoryService service)
        : base(service)
    {
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] ServiceCategoryInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] ServiceCategoryUpdateRequest request)
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
