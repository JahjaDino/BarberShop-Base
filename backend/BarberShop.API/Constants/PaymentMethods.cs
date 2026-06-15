namespace BarberShop.API.Constants;

public static class PaymentMethods
{
    public const string CASH_ON_SITE = "CASH_ON_SITE";
    public const string CARD_ON_SITE = "CARD_ON_SITE";

    public static readonly IReadOnlyCollection<string> OnSite =
    [
        CASH_ON_SITE,
        CARD_ON_SITE
    ];
}
