using BarberShop.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BarberShop.API.Services.Auth;

public class PasswordResetTokenCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly PasswordResetTokenCleanupSettings _settings;
    private readonly ILogger<PasswordResetTokenCleanupService> _logger;

    public PasswordResetTokenCleanupService(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<PasswordResetTokenCleanupSettings> options,
        ILogger<PasswordResetTokenCleanupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Password reset token cleanup is disabled.");
            return;
        }

        await CleanupAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(GetIntervalMinutes()));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CleanupAsync(stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        try
        {
            var now = DateTime.UtcNow;
            var retentionCutoff = now.AddDays(-GetRetentionDays());

            using var scope = _serviceScopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BarberShopDbContext>();

            var deletedCount = await dbContext.PasswordResetTokens
                .Where(token => token.ExpiresAt <= now
                    || token.UsedAt != null
                    || token.CreatedAt <= retentionCutoff)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation(
                    "Password reset token cleanup removed {DeletedCount} tokens.",
                    deletedCount);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Password reset token cleanup failed.");
        }
    }

    private int GetIntervalMinutes()
    {
        return _settings.IntervalMinutes > 0 ? _settings.IntervalMinutes : 60;
    }

    private int GetRetentionDays()
    {
        return _settings.RetentionDays > 0 ? _settings.RetentionDays : 7;
    }
}
