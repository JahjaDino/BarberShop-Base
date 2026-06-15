namespace BarberShop.API.Services.Security;

public interface ICurrentUserService
{
    int? CurrentUserId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }
}
