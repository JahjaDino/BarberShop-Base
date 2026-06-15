namespace BarberShop.API.Services.Auth;

public class AuthSettings
{
    public string ResetPasswordUrl { get; set; } = string.Empty;

    public int PasswordResetTokenExpirationMinutes { get; set; } = 30;

    public int ForgotPasswordMinimumResponseMilliseconds { get; set; } = 300;
}
