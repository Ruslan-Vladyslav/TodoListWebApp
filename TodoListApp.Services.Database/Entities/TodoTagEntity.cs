using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Database.Entities;

public class TodoTagEntity
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "A name is required for the tag")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string? Name { get; set; }

    public ICollection<TodoTaskEntity> TodoTasks { get; } = new List<TodoTaskEntity>();
}
