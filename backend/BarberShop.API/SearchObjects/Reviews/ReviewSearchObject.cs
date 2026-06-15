using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Reviews;

public class ReviewSearchObject : BaseSearchObject
{
    public int? AppointmentId { get; set; }

    public int? Rating { get; set; }

    public DateTimeOffset? DateFrom { get; set; }

    public DateTimeOffset? DateTo { get; set; }
}
