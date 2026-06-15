using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Owner;

public class OwnerAppointmentSearchObject : BaseSearchObject
{
    public DateOnly? Date { get; set; }

    public int? EmployeeId { get; set; }

    public string? Status { get; set; }
}
