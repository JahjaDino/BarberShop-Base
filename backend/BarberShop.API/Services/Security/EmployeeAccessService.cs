using BarberShop.API.Data;
using BarberShop.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarberShop.API.Services.Security;

public class EmployeeAccessService : IEmployeeAccessService
{
    private readonly BarberShopDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeAccessService(BarberShopDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Employee?> GetCurrentEmployeeAsync()
    {
        var currentUserId = _currentUserService.CurrentUserId;
        if (!currentUserId.HasValue)
        {
            return null;
        }

        return await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.UserId == currentUserId.Value && employee.Active);
    }

    public async Task<bool> CanManageAppointmentAsync(int appointmentEmployeeId)
    {
        var currentEmployee = await GetCurrentEmployeeAsync();

        return currentEmployee is not null && currentEmployee.Id == appointmentEmployeeId;
    }
}
