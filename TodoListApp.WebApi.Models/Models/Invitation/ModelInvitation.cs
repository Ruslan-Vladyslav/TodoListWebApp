using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Invitation;

public class ModelInvitation
{
    public int Id { get; set; }

    public int TodoListId { get; set; }

    public string TodoListTitle { get; set; } = null!;

    public string SenderUserId { get; set; } = null!;

    public string ReceiverUserId { get; set; } = null!;

    public string SenderUserName { get; set; } = null!;

    public string ReceiverUserName { get; set; } = null!;

    public TodoListRole Role { get; set; }

    public InvitationStatus Status { get; set; }

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }
}
