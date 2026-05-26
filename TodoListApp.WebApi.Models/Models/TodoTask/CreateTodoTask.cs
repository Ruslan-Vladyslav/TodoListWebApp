using System.ComponentModel.DataAnnotations;
using TodoListApp.Services.Enums;

namespace TodoListApp.WebApi.Models.Models.TodoTask;

public class CreateTodoTask
{
    [Required(ErrorMessage = "A title is required for the task")]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A description is required for the task")]
    [MaxLength(700)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime DueDate { get; set; } = DateTime.Now;

    public string UserId { get; set; } = string.Empty;

    public string? AssignedUserId { get; set; }

    public TodoTaskStatus Status { get; set; }

    [Required(ErrorMessage = "Please select a Todo List")]
    public int TodoListId { get; set; }
}
