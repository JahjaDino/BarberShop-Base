using BarberShop.API.Entities;

namespace BarberShop.API.Services.Security;

public interface IEmployeeAccessService
{
    Task<Employee?> GetCurrentEmployeeAsync();

    Task<bool> CanManageAppointmentAsync(int appointmentEmployeeId);
}
