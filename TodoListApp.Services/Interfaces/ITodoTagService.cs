using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Interfaces;

public interface ITodoTagService
{
    Task<ModelTodoTag> CreateTagAsync(string tagName);

    Task<IEnumerable<ModelTodoTag>> GetAllTagsAsync(int page, int pageSize);

    Task<ModelTodoTag?> GetByIdTagAsync(int id);

    Task<IEnumerable<ModelTodoTask>> GetTasksByTagAsync(int tagId);

    Task AddTagToTaskAsync(int taskId, int tagId);

    Task DeleteTagFromTaskAsync(int taskId, int tagId);
}
