namespace BarberShop.API.Services.Email;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "Email sending is disabled. Skipping email to {Email}. Subject: {Subject}",
                message.To,
                message.Subject);

            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("Email sending is enabled, but EmailSettings:Host is missing.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            _logger.LogWarning("Email sending is enabled, but EmailSettings:FromEmail is missing.");
            return;
        }

        if (!TryCreateMailboxAddress(message.To, out var toAddress))
        {
            _logger.LogWarning("Email recipient address is missing or invalid. To: {Email}", message.To);
            return;
        }

        using var smtpClient = new SmtpClient();

        try
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            email.To.Add(toAddress);
            email.Subject = message.Subject;
            email.Body = new TextPart("plain")
            {
                Text = message.Body
            };

            var secureSocketOptions = _settings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtpClient.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);

            if (!string.IsNullOrWhiteSpace(_settings.Username)
                && !string.IsNullOrWhiteSpace(_settings.Password))
            {
                await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password);
            }

            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);

            _logger.LogInformation(
                "Email sent to {Email}. Subject: {Subject}",
                message.To,
                message.Subject);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Email sending failed for {Email}. Subject: {Subject}",
                message.To,
                message.Subject);
        }
        finally
        {
            if (smtpClient.IsConnected)
            {
                await smtpClient.DisconnectAsync(true);
            }
        }
    }

    private static bool TryCreateMailboxAddress(string email, out MailboxAddress mailboxAddress)
    {
        mailboxAddress = null!;

        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            mailboxAddress = MailboxAddress.Parse(email);
            return true;
        }
        catch (ParseException)
        {
            return false;
        }
    }
}
