using BarberShop.API.Entities;

namespace BarberShop.API.Services.Appointments;

public static class AppointmentOverlapPolicy
{
    public static bool IsSlotAvailable(
        IReadOnlyCollection<Service> requestedServices,
        IReadOnlyCollection<Appointment> overlappingAppointments)
    {
        if (requestedServices.Any(service => !service.AllowOverlap))
        {
            return overlappingAppointments.Count == 0;
        }

        if (overlappingAppointments.Count == 0)
        {
            return true;
        }

        var existingNonOverlap = overlappingAppointments.Any(appointment =>
            appointment.AppointmentServices.Any(appointmentService =>
                appointmentService.Service is null || !appointmentService.Service.AllowOverlap));

        if (existingNonOverlap)
        {
            return false;
        }

        var parallelLimit = requestedServices.Min(GetEffectiveMaxParallelAppointments);
        return overlappingAppointments.Count < parallelLimit;
    }

    public static int GetEffectiveMaxParallelAppointments(Service service)
    {
        if (!service.AllowOverlap)
        {
            return 1;
        }

        return Math.Clamp(service.MaxParallelAppointments, 1, 3);
    }
}
