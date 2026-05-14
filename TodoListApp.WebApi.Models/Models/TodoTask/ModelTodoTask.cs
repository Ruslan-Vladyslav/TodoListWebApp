using System.ComponentModel.DataAnnotations;
using TodoListApp.Services.Enums;
using TodoListApp.WebApi.Models.Models.TodoComment;
using TodoListApp.WebApi.Models.Models.TodoTag;

namespace TodoListApp.WebApi.Models.Models.TodoTask;

public class ModelTodoTask
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required for the task")]
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [Required(ErrorMessage = "A description is required for the task")]
    [MaxLength(700)]
    public string? Description { get; set; }

    public DateTime CreateDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public TodoTaskStatus Status { get; set; } = TodoTaskStatus.NotStarted;

    public string UserId { get; set; } = string.Empty;

    public string? AssignedUserId { get; set; }

    public int TodoListId { get; set; }

    public IEnumerable<ModelTodoTag> Tags { get; set; } = new List<ModelTodoTag>();

    public IEnumerable<ModelTodoComment> Comments { get; set; } = new List<ModelTodoComment>();
}
