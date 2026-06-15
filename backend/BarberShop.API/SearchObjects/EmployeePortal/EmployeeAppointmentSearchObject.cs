namespace BarberShop.API.SearchObjects.EmployeePortal;

public class EmployeeAppointmentSearchObject : BaseSearchObject
{
    public DateOnly? Date { get; set; }

    public string? Status { get; set; }
}
