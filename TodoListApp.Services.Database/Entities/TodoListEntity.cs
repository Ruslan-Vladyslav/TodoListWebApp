using System.ComponentModel.DataAnnotations;
using TodoListApp.Services.Database.Entities;

namespace TodoListApp.Services.Database.Entity;

public class TodoListEntity
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required for the todo-list")]
    [StringLength(150, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 150 characters")]
    public string Title { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "Description must not be more than 500 characters.")]
    public string? Description { get; set; }

    public string? UserId { get; set; }

    public ICollection<TodoTaskEntity> TodoTasks { get; set; } = new List<TodoTaskEntity>();

    public ICollection<TodoListAccessEntity> Accesses { get; set; } = new List<TodoListAccessEntity>();
}
