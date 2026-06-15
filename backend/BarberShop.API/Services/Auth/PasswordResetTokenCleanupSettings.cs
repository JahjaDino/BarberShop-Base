namespace BarberShop.API.Services.Auth;

public class PasswordResetTokenCleanupSettings
{
    public bool Enabled { get; set; } = true;

    public int IntervalMinutes { get; set; } = 60;

    public int RetentionDays { get; set; } = 7;
}
