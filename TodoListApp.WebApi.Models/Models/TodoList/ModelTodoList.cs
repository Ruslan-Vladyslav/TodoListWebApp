using System.ComponentModel.DataAnnotations;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.WebApi.Models.Models.TodoList;

public class ModelTodoList
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required for the todo-list")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 150 characters")]
    public string? Title { get; set; }

    public string? UserId { get; set; }

    [MaxLength(500, ErrorMessage = "Description must not be more than 500 characters.")]
    public string? Description { get; set; }

    public IEnumerable<ModelTodoTask>? TodoTasks { get; set; }
}
