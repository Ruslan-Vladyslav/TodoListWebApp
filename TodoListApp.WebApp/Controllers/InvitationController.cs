using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class InvitationController : Controller
{
    private readonly IInvitationService _invitationService;
    private readonly IUserService _userService;

    public InvitationController(
        IInvitationService invitationService,
        IUserService userService)
    {
        _invitationService = invitationService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var invitations =
            await _invitationService.GetUserInvitationsAsync(userId);

        var userIds = invitations
            .Select(x => x.SenderUserId)
            .Concat(invitations.Select(x => x.ReceiverUserId))
            .Distinct()
            .ToList();

        var users = await _userService.GetUsersByIdsAsync(userIds);

        foreach (var item in invitations)
        {
            item.SenderUserName =
                users.GetValueOrDefault(item.SenderUserId) ?? "Unknown";

            item.ReceiverUserName =
                users.GetValueOrDefault(item.ReceiverUserId) ?? "Unknown";
        }

        var ordered = invitations
               .OrderBy(x => x.Status != InvitationStatus.Pending)
               .ThenByDescending(x => x.CreatedAt);

        return View(ordered);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        await _invitationService.AcceptInvitationAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        await _invitationService.RejectInvitationAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
