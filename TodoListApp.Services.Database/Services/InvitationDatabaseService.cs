using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.Services.Database.Services;

public class InvitationDatabaseService : IInvitationService
{
    private readonly TodoListDbContext _context;
    private readonly IAccessService _accessService;
    private readonly INotificationService _notificationService;

    public InvitationDatabaseService(
        TodoListDbContext context,
        IAccessService accessService,
        INotificationService notificationService)
    {
        this._context = context;
        this._accessService = accessService;
        this._notificationService = notificationService;
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
            await this._context.TodoLists
            .FirstOrDefaultAsync(x => x.Id == listId);

        if (list == null)
        {
            throw new KeyNotFoundException(
                "Todo list not found");
        }

        if (list.UserId != senderId)
        {
            throw new UnauthorizedAccessException("Only owner can share list");
        }

        var alreadyHasAccess =
            await this._context.TodoListAccesses
            .AnyAsync(x =>
                x.TargetUserId == receiverId &&
                x.TodoListId == listId);

        if (alreadyHasAccess)
        {
            throw new InvalidOperationException(
                "User already has access");
        }

        var pendingInvitation =
            await this._context.TodoInvitations
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
                Status = InvitationStatus.Pending,
                Message = message,
                CreatedAt = DateTime.UtcNow,
            };

        this._context.TodoInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        _ = await this._notificationService.CreateAsync(
           NotificationFactory.InvitationSent(
            receiverId,
            list.Title,
            list.Id));
    }

    public async Task AcceptInvitationAsync(int invitationId)
    {
        using var transaction = await this._context.Database.BeginTransactionAsync();

        var invite = await this._context.TodoInvitations
            .Include(x => x.TodoList)
            .FirstOrDefaultAsync(x => x.Id == invitationId);

        if (invite == null)
        {
            throw new KeyNotFoundException("Invitation not found");
        }

        if (invite.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Already processed");
        }

        invite.Status = InvitationStatus.Accepted;
        invite.RespondedAt = DateTime.UtcNow;

        await this._accessService.GrantAccessAsync(
            invite.SenderUserId,
            invite.ReceiverUserId,
            invite.TodoListId,
            invite.Role);

        _ = await this._notificationService.CreateAsync(
            NotificationFactory.InvitationAccepted(
            invite.SenderUserId,
            invite.ReceiverUserId,
            invite.TodoList.Title,
            invite.TodoListId));

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RejectInvitationAsync(
        int invitationId)
    {
        var invite =
            await this._context.TodoInvitations
            .Include(x => x.TodoList)
            .FirstOrDefaultAsync(x => x.Id == invitationId);

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

        invite.Status = InvitationStatus.Rejected;
        invite.RespondedAt = DateTime.UtcNow;

        await this._context.SaveChangesAsync();

        _ = await this._notificationService.CreateAsync(
            NotificationFactory.InvitationRejected(
            invite.SenderUserId,
            invite.ReceiverUserId,
            invite.TodoList.Title,
            invite.TodoListId));
    }

    public async Task<IEnumerable<ModelInvitation>> GetUserInvitationsAsync(string userId)
    {
        var items = await this._context.TodoInvitations
            .Include(x => x.TodoList)
            .Where(x => x.ReceiverUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return items.Select(x => new ModelInvitation
        {
            Id = x.Id,
            TodoListId = x.TodoListId,
            TodoListTitle = x.TodoList.Title,

            SenderUserId = x.SenderUserId,
            ReceiverUserId = x.ReceiverUserId,

            Role = x.Role,
            Status = x.Status,
            Message = x.Message,
            CreatedAt = x.CreatedAt,
        });
    }
}
