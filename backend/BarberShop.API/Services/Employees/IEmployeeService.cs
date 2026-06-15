using BarberShop.API.DTOs.Employees;
using BarberShop.API.SearchObjects.Employees;
using BarberShop.API.Services.Base;

namespace BarberShop.API.Services.Employees;

public interface IEmployeeService : IBaseCRUDService<EmployeeDto, EmployeeSearchObject, EmployeeInsertRequest, EmployeeUpdateRequest>
{
}
