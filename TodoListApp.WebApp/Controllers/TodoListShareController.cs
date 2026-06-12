using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Access;
using TodoListApp.WebApi.Models.Models.User;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoListShareController : Controller
{
    private readonly IInvitationService _invitationService;

    private readonly IAccessService _accessService;

    private readonly ITodoListService _todoListService;

    private readonly IUserService _userService;

    public TodoListShareController(
        IInvitationService invitationService,
        IAccessService accessService,
        ITodoListService todoListService,
        IUserService userService)
    {
        _invitationService = invitationService;
        _accessService = accessService;
        _todoListService = todoListService;
        this._userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> SharedWithMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var lists = await _todoListService.GetAllListByUserAsync(1, int.MaxValue, userId!);

        var shared = lists.Items
            .Where(x => x.UserId != userId) 
            .ToList();

        return View(shared);
    }


    [HttpGet]
    public async Task<IActionResult> Share(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        if (list.UserId != currentUserId)
        {
            return Forbid();
        }

        var vm = new ShareTodoListViewModel
        {
            ListId = list.Id,
            Title = list.Title,
            Description = list.Description
        };

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Share(ShareTodoListViewModel model)
    {
        var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var list = await _todoListService.GetByIdListAsync(model.ListId);

        if (list == null)
        {
            return NotFound();
        }

        if (list.UserId != senderId)
        {
            return Forbid();
        }

        model.Title = list.Title ?? model.Title;
        model.Description = list.Description ?? model.Description;

        if (string.IsNullOrWhiteSpace(model.ReceiverInput))
        {
            ModelState.AddModelError(nameof(model.ReceiverInput), "User is required.");
            return View(model);
        }

        var receiver = await FindUserAsync(model.ReceiverInput.Trim());

        if (receiver == null)
        {
            ModelState.AddModelError(
                nameof(model.ReceiverInput),
                "User not found. Enter a valid username or email.");
            return View(model);
        }

        if (receiver.Id == senderId)
        {
            ModelState.AddModelError(
                nameof(model.ReceiverInput),
                "You cannot invite yourself.");
            return View(model);
        }

        var existingRole = await _accessService.GetUserRoleAsync(receiver.Id, model.ListId);
        if (existingRole != null)
        {
            ModelState.AddModelError(
                nameof(model.ReceiverInput),
                "This user is already a member of this list.");
            return View(model);
        }

        if (await _invitationService.HasPendingInvitationAsync(model.ListId, receiver.Id))
        {
            ModelState.AddModelError(
                nameof(model.ReceiverInput),
                "An invitation has already been sent to this user.");
            return View(model);
        }

        if (!Enum.TryParse<TodoListRole>(model.Role, out var role))
        {
            role = TodoListRole.Viewer;
        }

        try
        {
            await _invitationService.SendInvitationAsync(
                senderId!,
                receiver.Id,
                model.ListId,
                role,
                model.Message);
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(
                nameof(model.ReceiverInput),
                MapInvitationError(ex));
            return View(model);
        }

        return RedirectToAction(nameof(Members), new { id = model.ListId });
    }

    [HttpGet]
    public async Task<IActionResult> Members(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        ViewBag.IsOwner = list.UserId == currentUserId;

        var accesses = await _accessService.GetAccessListAsync(id);
        var userIds = accesses.Select(x => x.TargetUserId).ToList();

        var allUsers = await _userService.GetAllAsync();

        var users = allUsers
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u.UserName);

        var owner = await _userService.GetByIdAsync(list.UserId!);

        var members = new List<MemberModel>
    {
        new MemberModel
        {
            UserId = list.UserId!,
            UserName = owner?.UserName ?? "Unknown",
            Role = "Owner"
        }
    };

        foreach (var access in accesses)
        {
            members.Add(new MemberModel
            {
                UserId = access.TargetUserId,
                UserName = users.TryGetValue(access.TargetUserId, out var name)
                    ? name
                    : "Unknown",
                Role = access.Role.ToString()
            });
        }

        var model = new ModelTodoListMembers
        {
            ListId = list.Id,
            Title = list.Title!,
            Members = members,
            OwnerUserId = list.UserId!,
        };

        ViewBag.CurrentUserId = currentUserId;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(int listId, string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var list = await _todoListService.GetByIdListAsync(listId);

        if (list == null)
        {
            return NotFound();
        }

        if (list.UserId != currentUserId)
        {
            return Forbid();
        }

        if (userId == list.UserId)
        {
            return BadRequest("Owner cannot be removed");
        }

        await _accessService.RevokeAccessAsync(currentUserId, userId, listId);
        return RedirectToAction("Members", new { id = listId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int listId, string userId, TodoListRole newRole)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var list = await _todoListService.GetByIdListAsync(listId);

        if (list == null)
        {
            return NotFound();
        }

        if (list.UserId != currentUserId)
        {
            return Forbid();
        }

        if (userId == list.UserId)
        {
            return BadRequest("Cannot change the owner's role.");
        }

        await _accessService.UpdateRoleAsync(currentUserId!, userId, listId, newRole);
        return RedirectToAction("Members", new { id = listId });
    }

    private async Task<UserModel?> FindUserAsync(string input)
    {
        var byEmail = await _userService.GetByEmailAsync(input);
        if (byEmail != null)
        {
            return byEmail;
        }

        return await _userService.GetByUserNameAsync(input);
    }

    private static string MapInvitationError(HttpRequestException ex)
    {
        var apiError = ParseApiError(ex);

        return apiError switch
        {
            "User already has access" =>
                "This user is already a member of this list.",
            "Pending invitation already exists" =>
                "An invitation has already been sent to this user.",
            "Cannot invite yourself" =>
                "You cannot invite yourself.",
            _ => string.IsNullOrWhiteSpace(apiError)
                ? "Could not send invitation."
                : apiError,
        };
    }

    private static string? ParseApiError(HttpRequestException ex)
    {
        var message = ex.Message;
        var jsonStart = message.IndexOf('{');
        if (jsonStart >= 0)
        {
            try
            {
                using var doc = JsonDocument.Parse(message[jsonStart..]);
                if (doc.RootElement.TryGetProperty("error", out var error))
                {
                    return error.GetString();
                }
            }
            catch (JsonException)
            {
            }
        }

        var colon = message.IndexOf(':');
        if (colon >= 0 && colon < message.Length - 1)
        {
            return message[(colon + 1)..].Trim();
        }
        return message;
    }
}
