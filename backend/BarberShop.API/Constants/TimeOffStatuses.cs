namespace BarberShop.API.Constants;

public static class TimeOffStatuses
{
    public const string PENDING = "PENDING";
    public const string APPROVED = "APPROVED";
    public const string REJECTED = "REJECTED";
    public const string CANCELLED = "CANCELLED";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        PENDING,
        APPROVED,
        REJECTED,
        CANCELLED
    };

    public static readonly IReadOnlyCollection<string> NonBlocking = new[]
    {
        REJECTED,
        CANCELLED
    };
}
