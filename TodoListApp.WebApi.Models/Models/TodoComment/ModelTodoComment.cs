using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models.Models.TodoComment;

public class ModelTodoComment
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Textshould contain 1-500 characters")]
    public string Text { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime CreateDate { get; set; }

    public int TodoTaskId { get; set; }
}
