using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.WebApp.Models;

public class TasksPartialViewModel
{
    public IEnumerable<ModelTodoTask> Tasks { get; set; } = new List<ModelTodoTask>();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public string? SearchType { get; set; }

    public int? TagId { get; set; }

    public string? Title { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; } = DateTime.Today.AddDays(1);

    public string Action { get; set; } = "Index";

    public string Controller { get; set; } = "TodoTag";
}
