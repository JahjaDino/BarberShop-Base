namespace BarberShop.API.DTOs.Auth;

public class CurrentUserResponse
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
