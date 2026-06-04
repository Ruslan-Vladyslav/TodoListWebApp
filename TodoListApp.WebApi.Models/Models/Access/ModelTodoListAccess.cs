using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Access;

public class ModelTodoListAccess
{
    public int TodoListId { get; set; }

    public string OwnerUserId { get; set; } = null!;

    public string TargetUserId { get; set; } = null!;

    public string? TargetUserName { get; set; }

    public TodoListRole Role { get; set; }

    public bool Accepted { get; set; }
}
