using BarberShop.API.DTOs.Appointments;

namespace BarberShop.API.Services.Appointments.Booking;

public interface IAppointmentBookingFacade
{
    Task<AppointmentDto> BookAsync(AppointmentBookRequest request);
}
