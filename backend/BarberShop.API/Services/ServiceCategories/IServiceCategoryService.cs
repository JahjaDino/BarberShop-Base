using BarberShop.API.DTOs.ServiceCategories;
using BarberShop.API.SearchObjects.ServiceCategories;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.ServiceCategories;

public interface IServiceCategoryService
    : IBaseCRUDService<ServiceCategoryDto, ServiceCategorySearchObject, ServiceCategoryInsertRequest, ServiceCategoryUpdateRequest>
{
}
