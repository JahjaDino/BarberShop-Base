using BarberShop.API.DTOs.Appointments;
using BarberShop.API.DTOs.Client;
using BarberShop.API.Models;
using BarberShop.API.SearchObjects.Appointments;

namespace BarberShop.API.Services.Appointments;

public interface IAppointmentManagementService
{
    Task<PagedResult<AppointmentDto>> GetAsync(AppointmentSearchObject search);

    Task<PagedResult<AppointmentDto>> GetMineAsync(AppointmentSearchObject search);

    Task<PagedResult<ClientAppointmentCardDto>> GetMyClientCardsAsync(ClientAppointmentSearchObject search);

    Task<PagedResult<AppointmentDto>> GetMyEmployeeAppointmentsAsync(AppointmentSearchObject search);

    Task<AppointmentDto?> GetByIdAsync(int id);

    Task<AppointmentDto> CancelAsync(int id, AppointmentCancelRequest request);

    Task<AppointmentDto?> UpdateStatusAsync(int id, AppointmentStatusUpdateRequest request);

    Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsAsync(int serviceId, int employeeId, DateOnly date);

    Task<IReadOnlyCollection<AvailableSlotDto>> GetAvailableSlotsAsync(int shopId, int serviceId, int employeeId, DateOnly date);
}
