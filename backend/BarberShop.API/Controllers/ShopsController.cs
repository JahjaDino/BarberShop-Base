using BarberShop.API.Constants;
using BarberShop.API.Controllers.Base;
using BarberShop.API.DTOs.Shops;
using BarberShop.API.SearchObjects.Shops;
using BarberShop.API.Services.Shops;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Route("api/shops")]
public class ShopsController : BaseCRUDController<ShopDto, ShopSearchObject, ShopInsertRequest, ShopUpdateRequest>
{
    public ShopsController(IShopService service)
        : base(service)
    {
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPost]
    public override Task<IActionResult> Create([FromBody] ShopInsertRequest request)
    {
        return base.Create(request);
    }

    [Authorize(Roles = RoleNames.OWNER)]
    [HttpPut("{id:int}")]
    public override Task<IActionResult> Update(int id, [FromBody] ShopUpdateRequest request)
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
