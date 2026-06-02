using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
internal class InvitationController : ControllerBase
{
    private readonly IInvitationService _service;

    public InvitationController(IInvitationService service)
    {
        this._service = service;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send(SendInvitationRequest request)
    {
        await this._service.SendInvitationAsync(
            request.SenderId,
            request.ReceiverId,
            request.ListId,
            request.Role,
            request.Message);

        return Ok();
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

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserInvitations(string userId)
    {
        var result = await this._service.GetUserInvitationsAsync(userId);
        return Ok(result);
    }
}
