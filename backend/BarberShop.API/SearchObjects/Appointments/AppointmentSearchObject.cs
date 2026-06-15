using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Appointments;

public class AppointmentSearchObject : BaseSearchObject
{
    public int? ClientId { get; set; }

    public int? EmployeeId { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset? DateFrom { get; set; }

    public DateTimeOffset? DateTo { get; set; }
}
