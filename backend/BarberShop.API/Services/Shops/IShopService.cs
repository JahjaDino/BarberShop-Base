using BarberShop.API.DTOs.Shops;
using BarberShop.API.SearchObjects.Shops;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.Shops;

public interface IShopService : IBaseCRUDService<ShopDto, ShopSearchObject, ShopInsertRequest, ShopUpdateRequest>
{
}
