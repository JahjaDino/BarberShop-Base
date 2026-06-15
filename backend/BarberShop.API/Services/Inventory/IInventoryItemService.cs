using BarberShop.API.DTOs.Inventory;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects.Inventory;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.Inventory;

public interface IInventoryItemService
    : IBaseCRUDService<InventoryItemDto, InventoryItemSearchObject, InventoryItemInsertRequest, InventoryItemUpdateRequest>
{
    Task<PagedResult<InventoryItemDto>> GetLowStockAsync(InventoryItemSearchObject search);
}
