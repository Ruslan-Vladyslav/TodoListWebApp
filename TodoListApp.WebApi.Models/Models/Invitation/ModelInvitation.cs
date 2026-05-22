using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Invitation;

public class ModelInvitation
{
    public int Id { get; set; }

    public string SenderUserName { get; set; } = null!;

    public string ReceiverUserName { get; set; } = null!;

    public TodoListRole Role { get; set; }

    public InvitationStatus Status { get; set; }

    public string? Message { get; set; }
}
