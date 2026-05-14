using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models.Models.TodoTask;

public class SearchTasksViewModel
{
    [Required(ErrorMessage = "Enter choose criteria")]
    public string SearchType { get; set; } = string.Empty;

    [Display(Name = "Searched text")]
    public string? Title { get; set; }

    [Display(Name = "Creation Date")]
    [DataType(DataType.Date)]
    public DateTime? CreateDate { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    public int? TagId { get; set; }

    public IEnumerable<ModelTodoTask>? Results { get; set; }
}
