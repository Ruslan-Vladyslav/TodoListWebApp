namespace TodoListApp.WebApp.Models;

public class ShareTodoListViewModel
{
    public int ListId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string? ReceiverInput { get; set; }
    public string Role { get; set; } = "Viewer";
    public string? Message { get; set; }
}
