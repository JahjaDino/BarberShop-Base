using BarberShop.API.DTOs.Auth;

namespace BarberShop.API.Services.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task<bool> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);

    Task<CurrentUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken);

    Task<CurrentUserResponse> UpdateProfileAsync(int userId, ProfileUpdateRequest request, CancellationToken cancellationToken);

    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken);

    Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);

    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
}
