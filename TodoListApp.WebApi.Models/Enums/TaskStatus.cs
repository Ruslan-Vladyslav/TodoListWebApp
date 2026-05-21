using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Services.Enums;

public enum TodoTaskStatus
{
    [Display(Name = "Not Started")]
    NotStarted = 0,

    [Display(Name = "In Progress")]
    InProgress = 1,

    [Display(Name = "Completed")]
    Completed = 2,
}
