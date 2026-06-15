using BarberShop.API.Entities;

namespace BarberShop.API.Services.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user, IReadOnlyCollection<string> roles);
}
