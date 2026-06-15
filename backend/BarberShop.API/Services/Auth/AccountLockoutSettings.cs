namespace BarberShop.API.Services.Auth;

public class AccountLockoutSettings
{
    public int MaxFailedAttempts { get; set; } = 5;

    public int[] DurationsMinutes { get; set; } = [1, 5, 15, 30, 60];
}
