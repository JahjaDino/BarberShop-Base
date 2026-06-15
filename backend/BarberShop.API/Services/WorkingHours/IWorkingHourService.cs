using BarberShop.API.DTOs.WorkingHours;
using BarberShop.API.SearchObjects.WorkingHours;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.WorkingHours;

public interface IWorkingHourService : IBaseCRUDService<WorkingHourDto, WorkingHourSearchObject, WorkingHourInsertRequest, WorkingHourUpdateRequest>
{
}
