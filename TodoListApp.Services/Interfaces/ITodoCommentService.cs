using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Services.Interfaces;

public interface ITodoCommentService
{
    Task<ModelTodoComment?> GetCommentByIdAsync(int id);

    Task<ModelTodoComment> CreateCommentAsync(int taskId, CreateTodoComment comment);

    Task DeleteCommentAsync(int id);
}
