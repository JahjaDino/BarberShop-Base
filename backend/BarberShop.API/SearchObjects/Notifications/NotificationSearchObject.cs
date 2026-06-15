using BarberShop.API.SearchObjects;

namespace BarberShop.API.SearchObjects.Notifications;

public class NotificationSearchObject : BaseSearchObject
{
    public string? Status { get; set; }

    public string? Type { get; set; }

    public DateTimeOffset? DateFrom { get; set; }

    public DateTimeOffset? DateTo { get; set; }
}
