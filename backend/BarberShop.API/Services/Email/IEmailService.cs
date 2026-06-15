namespace BarberShop.API.Services.Email;

public interface IEmailService
{
    Task SendAsync(EmailMessage message);
}
