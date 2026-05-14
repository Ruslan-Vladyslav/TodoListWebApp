using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models.Models.TodoTag;

public class CreateTodoTag
{
    [Required(ErrorMessage = "A name is required for the tag")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
    public string? Name { get; set; }

    public int TodoTaskId { get; set; }
}
