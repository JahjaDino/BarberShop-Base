using BarberShop.API.DTOs.EmployeePortal;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects.EmployeePortal;

namespace BarberShop.API.Services.EmployeePortal;

public interface IEmployeePortalService
{
    Task<EmployeeDashboardDto> GetDashboardAsync(DateOnly? date);

    Task<EmployeeScheduleDto> GetScheduleAsync(DateOnly? date);

    Task<PagedResult<EmployeeAppointmentListItemDto>> GetAppointmentsAsync(EmployeeAppointmentSearchObject search);

    Task<IReadOnlyCollection<EmployeeTimeOffDto>> GetTimeOffAsync();

    Task<EmployeeTimeOffDto> CreateTimeOffAsync(EmployeeTimeOffCreateRequest request);

    Task<IReadOnlyCollection<EmployeeServiceDto>> GetServicesAsync();

    Task<IReadOnlyCollection<EmployeeInventoryItemDto>> GetInventoryAsync();

    Task<EmployeeInventoryItemDto> ReportInventoryAsync(EmployeeInventoryReportRequest request);

    Task<EmployeeProfileDto> GetProfileAsync();

    Task<EmployeeProfileDto> UpdateProfileAsync(EmployeeProfileUpdateRequest request);
}
