using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Invitation;

public class CreateInvitation
{
    public string ReceiverUserId { get; set; } = null!;

    public int TodoListId { get; set; }

    public TodoListRole Role { get; set; }

    public string? Message { get; set; }
}
