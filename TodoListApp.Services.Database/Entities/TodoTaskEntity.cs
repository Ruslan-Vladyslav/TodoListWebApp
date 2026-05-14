using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Services.Database.Entity;
using TodoListApp.Services.Enums;

namespace TodoListApp.Services.Database.Entities;

public class TodoTaskEntity
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required for the task")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 150 characters")]
    public string? Title { get; set; }

    [Required(ErrorMessage = "A description is required for the task")]
    [MaxLength(500, ErrorMessage = "Description must not be more than 500 characters.")]
    public string? Description { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.Now;

    [Required]
    public DateTime DueDate { get; set; }

    public string? UserId { get; set; }

    public string? AssignedUserId { get; set; }

    [Required]
    public TodoTaskStatus Status { get; set; } = TodoTaskStatus.NotStarted;

    [ForeignKey(nameof(TodoList))]
    public int TodoListId { get; set; }

    public TodoListEntity? TodoList { get; set; }

    public ICollection<TodoTagEntity>? Tags { get; set; } = new List<TodoTagEntity>();

    public ICollection<TodoCommentEntity>? Comments { get; set; } = new List<TodoCommentEntity>();
}
