using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.WebApi.Models.Models.TodoTag;

public class TagsTasksViewModel
{
    public IEnumerable<ModelTodoTag> Tags { get; set; } = Enumerable.Empty<ModelTodoTag>();

    public IEnumerable<ModelTodoTask> Tasks { get; set; } = Enumerable.Empty<ModelTodoTask>();

    public int? SelectedTagId { get; set; }
}
