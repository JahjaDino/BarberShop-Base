namespace BarberShop.API.DTOs.Auth;

public class LoginResult
{
    public AuthResponse? AuthResponse { get; set; }

    public bool IsLocked { get; set; }

    public bool Succeeded => AuthResponse is not null;
}
