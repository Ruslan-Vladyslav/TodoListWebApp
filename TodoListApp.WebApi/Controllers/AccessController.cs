using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
internal class AccessController : ControllerBase
{
    private readonly IAccessService _service;

    public AccessController(IAccessService service)
    {
        this._service = service;
    }

    [HttpPost("grant")]
    public async Task<IActionResult> Grant(
    [FromQuery] string userId,
    [FromQuery] int listId,
    [FromQuery] TodoListRole role)
    {
        var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._service.GrantAccessAsync(ownerUserId!, userId, listId, role);
        return Ok();
    }

    [HttpDelete("revoke")]
    public async Task<IActionResult> Revoke(
    [FromQuery] string targetUserId,
    [FromQuery] int listId)
    {
        var ownerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._service.RevokeAccessAsync(
            ownerUserId!,
            targetUserId,
            listId);

        return NoContent();
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers([FromQuery] int listId)
    {
        var items = await this._service
            .GetAccessListAsync(listId);

        return Ok(items);
    }

    [HttpGet("role")]
    public async Task<IActionResult> GetRole([FromQuery] string userId, [FromQuery] int listId)
    {
        var role = await this._service.GetUserRoleAsync(userId, listId);
        return Ok(role);
    }
}
