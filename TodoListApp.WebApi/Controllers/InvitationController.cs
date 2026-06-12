using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class InvitationController : ControllerBase
{
    private readonly IInvitationService _service;

    public InvitationController(IInvitationService service)
    {
        this._service = service;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(SendInvitationRequest request)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._service.SendInvitationAsync(
            senderId!,
            request.ReceiverId,
            request.ListId,
            request.Role,
            request.Message);

        return Ok();
    }

    [HttpGet("pending")]
    public async Task<IActionResult> HasPending(
        [FromQuery] int listId,
        [FromQuery] string receiverId)
    {
        var hasPending = await this._service.HasPendingInvitationAsync(listId, receiverId);
        return Ok(hasPending);
    }

    [HttpPost("accept/{id}")]
    public async Task<IActionResult> Accept(int id)
    {
        await this._service.AcceptInvitationAsync(id);
        return Ok();
    }

    [HttpPost("reject/{id}")]
    public async Task<IActionResult> Reject(int id)
    {
        await this._service.RejectInvitationAsync(id);
        return Ok();
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserInvitations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await this._service.GetUserInvitationsAsync(userId!);
        return Ok(result);
    }
}
