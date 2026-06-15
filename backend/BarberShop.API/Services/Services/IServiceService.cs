using BarberShop.API.DTOs.Services;
using BarberShop.API.SearchObjects.Services;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.Services;

public interface IServiceService : IBaseCRUDService<ServiceDto, ServiceSearchObject, ServiceInsertRequest, ServiceUpdateRequest>
{
}
