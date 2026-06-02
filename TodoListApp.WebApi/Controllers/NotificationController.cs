using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Notification;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("notifications")]
internal class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        this._service = service;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(string userId)
    {
        var result = await this._service.GetUserNotificationsAsync(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotification request)
    {
        var result = await this._service.CreateAsync(request);
        return Ok(result);
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await this._service.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await this._service.DeleteAsync(id);
        return NoContent();
    }
}
