using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Services.Database.Entities;

public class TodoCommentEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Textshould contain 1-150 characters")]
    public string Text { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(TodoTask))]
    public int TodoTaskId { get; set; }

    public TodoTaskEntity? TodoTask { get; set; }
}
