using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models;

public class SearchViewModel
{
    public string? Title { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Date { get; set; }
}
