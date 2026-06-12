using System.ComponentModel.DataAnnotations;
using TodoListApp.Services.Enums;
using TodoListApp.WebApi.Models.Models.TodoComment;
using TodoListApp.WebApi.Models.Models.TodoTag;

namespace TodoListApp.WebApi.Models.Models.TodoTask;

public class UpdateTodoTask
{
    public int Id { get; set; }

    [Required(ErrorMessage = "A title is required for the task")]
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [Required(ErrorMessage = "A description is required for the task")]
    [MaxLength(700)]
    public string? Description { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
    public DateTime DueDate { get; set; } = DateTime.Now;

    public TodoTaskStatus Status { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? AssignedUserId { get; set; }

    public string? AssignedUserName { get; set; }

    public int TodoListId { get; set; }

    public IEnumerable<ModelTodoTag>? Tags { get; set; }

    public IEnumerable<ModelTodoComment>? Comments { get; set; }
}
