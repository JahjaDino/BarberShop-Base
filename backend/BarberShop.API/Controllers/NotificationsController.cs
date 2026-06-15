using BarberShop.API.Constants;
using BarberShop.API.SearchObjects.Notifications;
using BarberShop.API.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberShop.API.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [Authorize(Roles = $"{RoleNames.CLIENT},{RoleNames.OWNER},{RoleNames.EMPLOYEE}")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMine([FromQuery] NotificationSearchObject search)
    {
        return Ok(await _notificationService.GetMyNotificationsAsync(search));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _notificationService.MarkAsReadAsync(id);

        return notification is null ? NotFound() : Ok(notification);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var updatedCount = await _notificationService.MarkAllAsReadAsync();

        return Ok(new { updatedCount });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _notificationService.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }

    [HttpDelete("read")]
    public async Task<IActionResult> DeleteRead()
    {
        var deletedCount = await _notificationService.DeleteReadAsync();

        return Ok(new { deletedCount });
    }
}
