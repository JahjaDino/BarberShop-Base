namespace BarberShop.API.Constants;

public static class NotificationTypes
{
    public const string APPOINTMENT_BOOKED = "APPOINTMENT_BOOKED";
    public const string APPOINTMENT_CONFIRMED = "APPOINTMENT_CONFIRMED";
    public const string APPOINTMENT_CANCELLED = "APPOINTMENT_CANCELLED";
    public const string APPOINTMENT_COMPLETED = "APPOINTMENT_COMPLETED";
    public const string APPOINTMENT_NO_SHOW = "APPOINTMENT_NO_SHOW";
    public const string TIME_OFF_REQUESTED = "TIME_OFF_REQUESTED";
    public const string TIME_OFF_APPROVED = "TIME_OFF_APPROVED";
    public const string TIME_OFF_REJECTED = "TIME_OFF_REJECTED";

    public static readonly IReadOnlyCollection<string> All =
    [
        APPOINTMENT_BOOKED,
        APPOINTMENT_CONFIRMED,
        APPOINTMENT_CANCELLED,
        APPOINTMENT_COMPLETED,
        APPOINTMENT_NO_SHOW,
        TIME_OFF_REQUESTED,
        TIME_OFF_APPROVED,
        TIME_OFF_REJECTED
    ];
}
