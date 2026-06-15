using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Appointments;

public class ClientAppointmentSearchObject : BaseSearchObject
{
    public string? Filter { get; set; }
}
