using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.Services.Interfaces;

public interface IInvitationService
{
    Task SendInvitationAsync(string senderId, string receiverId, int listId, TodoListRole role, string? message = null);

    Task AcceptInvitationAsync(int invitationId);

    Task RejectInvitationAsync(int invitationId);

    Task<IEnumerable<ModelInvitation>> GetUserInvitationsAsync(string userId);
}
