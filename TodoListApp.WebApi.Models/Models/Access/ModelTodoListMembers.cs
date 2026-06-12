namespace TodoListApp.WebApi.Models.Models.Access;

public class ModelTodoListMembers
{
    public int ListId { get; set; }

    public string Title { get; set; } = null!;

    public string OwnerUserId { get; set; } = null!;

    public List<MemberModel> Members { get; set; } = new();
}
