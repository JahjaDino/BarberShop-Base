using BarberShop.API.Services.Notifications.Strategies;

namespace BarberShop.API.Services.Notifications.Factories;

public interface INotificationStrategyFactory
{
    INotificationStrategy GetStrategy(string type);
}
