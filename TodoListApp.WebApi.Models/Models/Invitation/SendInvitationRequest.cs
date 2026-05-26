using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Invitation;

public class SendInvitationRequest
{
    public string SenderId { get; set; } = null!;

    public string ReceiverId { get; set; } = null!;

    public int ListId { get; set; }

    public TodoListRole Role { get; set; }

    public string? Message { get; set; }
}
