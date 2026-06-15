namespace BarberShop.API.Services.Auth;

public class AuthRateLimitSettings
{
    public EndpointRateLimitSettings Login { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 60
    };

    public EndpointRateLimitSettings Register { get; set; } = new()
    {
        PermitLimit = 10,
        WindowSeconds = 300
    };

    public EndpointRateLimitSettings ForgotPassword { get; set; } = new()
    {
        PermitLimit = 3,
        WindowSeconds = 300
    };

    public EndpointRateLimitSettings ResetPassword { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 300
    };

    public EndpointRateLimitSettings AppointmentBooking { get; set; } = new()
    {
        PermitLimit = 5,
        WindowSeconds = 600
    };
}

public class EndpointRateLimitSettings
{
    public int PermitLimit { get; set; } = 5;

    public int WindowSeconds { get; set; } = 60;

    public int QueueLimit { get; set; }
}
