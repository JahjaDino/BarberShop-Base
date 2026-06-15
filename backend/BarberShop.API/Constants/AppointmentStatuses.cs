namespace BarberShop.API.Constants;

public static class AppointmentStatuses
{
    public const string PENDING = "PENDING";
    public const string CONFIRMED = "CONFIRMED";
    public const string COMPLETED = "COMPLETED";
    public const string CANCELLED = "CANCELLED";
    public const string NO_SHOW = "NO_SHOW";

    public static readonly IReadOnlyCollection<string> All =
    [
        PENDING,
        CONFIRMED,
        COMPLETED,
        CANCELLED,
        NO_SHOW
    ];

    public static readonly IReadOnlyCollection<string> Blocking =
    [
        PENDING,
        CONFIRMED
    ];

    public static readonly IReadOnlyCollection<string> Terminal =
    [
        COMPLETED,
        CANCELLED,
        NO_SHOW
    ];

    public static bool CanTransition(string currentStatus, string newStatus)
    {
        return currentStatus switch
        {
            PENDING => newStatus is CONFIRMED or CANCELLED,
            CONFIRMED => newStatus is COMPLETED or CANCELLED or NO_SHOW,
            _ => false
        };
    }
}
