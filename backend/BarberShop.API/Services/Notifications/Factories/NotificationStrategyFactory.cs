using BarberShop.API.Exceptions;
using BarberShop.API.Services.Notifications.Strategies;

namespace BarberShop.API.Services.Notifications.Factories;

public class NotificationStrategyFactory : INotificationStrategyFactory
{
    private readonly IReadOnlyDictionary<string, INotificationStrategy> _strategies;

    public NotificationStrategyFactory(IEnumerable<INotificationStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(
            strategy => strategy.Type,
            strategy => strategy,
            StringComparer.OrdinalIgnoreCase);
    }

    public INotificationStrategy GetStrategy(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new BadRequestException("Notification type is required.");
        }

        if (!_strategies.TryGetValue(type.Trim().ToUpper(), out var strategy))
        {
            throw new BadRequestException("Notification type is not valid.");
        }

        return strategy;
    }
}
