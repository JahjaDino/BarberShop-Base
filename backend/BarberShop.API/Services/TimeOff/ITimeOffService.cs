using BarberShop.API.DTOs.TimeOff;
using BarberShop.API.SearchObjects.TimeOff;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.TimeOff;

public interface ITimeOffService : IBaseCRUDService<TimeOffDto, TimeOffSearchObject, TimeOffInsertRequest, TimeOffUpdateRequest>
{
    Task<TimeOffDto?> UpdateStatusAsync(int id, TimeOffStatusUpdateRequest request);
}
