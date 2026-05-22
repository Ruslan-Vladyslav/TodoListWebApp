using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.Services.Interfaces;

public interface IInvitationService
{
    Task SendInvitationAsync(string senderId, string receiverId, int listId, TodoListRole role, string? message = null);

    Task AcceptInvitationAsync(int invitationId);

    Task RejectInvitationAsync(int invitationId);
}
