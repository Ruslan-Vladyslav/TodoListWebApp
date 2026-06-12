using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _service;

    public NotificationController(
        INotificationService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications =
            await _service.GetUserNotificationsAsync(userId!);

        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notification = (await _service.GetUserNotificationsAsync(userId!))
            .FirstOrDefault(x => x.Id == id);

        if (notification == null)
        {
            return NotFound();
        }

        await _service.MarkAsReadAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications =
            await _service.GetUserNotificationsAsync(userId!);

        foreach (var notification in notifications.Where(x => !x.IsRead))
        {
            await _service.MarkAsReadAsync(notification.Id);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notification = (await _service.GetUserNotificationsAsync(userId!))
            .FirstOrDefault(x => x.Id == id);

        if (notification == null)
        {
            return NotFound();
        }

        await _service.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected(List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            return RedirectToAction(nameof(Index));
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notifications =
            await _service.GetUserNotificationsAsync(userId!);

        var allowedIds = notifications
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var id in ids.Where(allowedIds.Contains))
        {
            await _service.DeleteAsync(id);
        }

        return RedirectToAction(nameof(Index));
    }
}
