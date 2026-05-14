using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models.Models.TodoList;

public class CreateTodoList
{
    [Required(ErrorMessage = "A title is required for the todo-list")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 150 characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Description must not be more than 500 characters.")]
    public string Description { get; set; } = string.Empty;

    public string? UserId { get; set; }
}
