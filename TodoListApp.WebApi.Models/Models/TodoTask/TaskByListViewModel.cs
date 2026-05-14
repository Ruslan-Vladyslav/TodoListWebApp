namespace TodoListApp.WebApi.Models.Models.TodoTask;

public class TaskByListViewModel
{
    public int TodoListId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IEnumerable<ModelTodoTask> Tasks { get; set; } = new List<ModelTodoTask>();
}
