using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.Services.Database.Services;

public class InvitationDatabaseService : IInvitationService
{
    private readonly TodoListDbContext _context;
    private readonly IAccessService _accessService;

    public InvitationDatabaseService(
        TodoListDbContext context,
        IAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task SendInvitationAsync(
        string senderId,
        string receiverId,
        int listId,
        TodoListRole role,
        string? message = null)
    {
        if (senderId == receiverId)
        {
            throw new InvalidOperationException(
                "Cannot invite yourself");
        }

        var list =
            await _context.TodoLists
            .FirstOrDefaultAsync(x => x.Id == listId);

        if (list == null)
        {
            throw new KeyNotFoundException(
                "Todo list not found");
        }

        if (list.UserId != senderId)
        {
            throw new UnauthorizedAccessException(
                "Only owner can share list");
        }

        var alreadyHasAccess =
            await _context.TodoListAccesses
            .AnyAsync(x =>
                x.TargetUserId == receiverId &&
                x.TodoListId == listId);

        if (alreadyHasAccess)
        {
            throw new InvalidOperationException(
                "User already has access");
        }

        var pendingInvitation =
            await _context.TodoInvitations
            .AnyAsync(x =>
                x.TodoListId == listId &&
                x.ReceiverUserId == receiverId &&
                x.Status == InvitationStatus.Pending);

        if (pendingInvitation)
        {
            throw new InvalidOperationException(
                "Pending invitation already exists");
        }

        var invitation =
            new TodoInvitationEntity
            {
                SenderUserId = senderId,
                ReceiverUserId = receiverId,

                TodoListId = listId,

                Role = role,

                Status =
                    InvitationStatus.Pending,

                Message = message,

                CreatedAt =
                    DateTime.UtcNow
            };

        _context.TodoInvitations.Add(
            invitation);

        await _context.SaveChangesAsync();
    }

    public async Task AcceptInvitationAsync(
        int invitationId)
    {
        using var transaction =
            await _context.Database
            .BeginTransactionAsync();

        var invite =
            await _context.TodoInvitations
            .FirstOrDefaultAsync(
                x => x.Id == invitationId);

        if (invite == null)
        {
            throw new KeyNotFoundException(
                "Invitation not found");
        }

        if (invite.Status !=
            InvitationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Invitation already processed");
        }

        invite.Status =
            InvitationStatus.Accepted;

        invite.RespondedAt =
            DateTime.UtcNow;

        await _accessService
            .GrantAccessAsync(
                invite.SenderUserId,
                invite.ReceiverUserId,
                invite.TodoListId,
                invite.Role);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    public async Task RejectInvitationAsync(
        int invitationId)
    {
        var invite =
            await _context.TodoInvitations
            .FirstOrDefaultAsync(
                x => x.Id == invitationId);

        if (invite == null)
        {
            throw new KeyNotFoundException(
                "Invitation not found");
        }

        if (invite.Status !=
            InvitationStatus.Pending)
        {
            throw new InvalidOperationException(
                "Invitation already processed");
        }

        invite.Status =
            InvitationStatus.Rejected;

        invite.RespondedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelInvitation>> GetUserInvitationsAsync(string userId)
    {
        return await _context.TodoInvitations
            .Include(x => x.TodoList)
            .Where(x => x.ReceiverUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync()
            .ContinueWith(t => t.Result.Select(x => new ModelInvitation
            {
                Id = x.Id,
                TodoListId = x.TodoListId,
                TodoListTitle = x.TodoList.Title,

                SenderUserId = x.SenderUserId,
                ReceiverUserId = x.ReceiverUserId,

                Role = x.Role,
                Status = x.Status,
                Message = x.Message,
                CreatedAt = x.CreatedAt
            }));
}
}
