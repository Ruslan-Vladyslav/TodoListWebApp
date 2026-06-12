using TodoListApp.Services.Enums;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Interfaces;

public interface ITodoTaskService
{
    Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item);

    Task<ModelTodoTask?> GetByIdTaskAsync(int id, string userId);

    Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId, string userId);

    Task<PagedResponse<ModelTodoTask>> GetAllTasksAsync(
        int page,
        int pageSize,
        int? todoListId,
        string? userId,
        TodoTaskStatus? status,
        string? sort);

    Task<PagedResponse<ModelTodoTask>> GetAllTasksByTitleAsync(
        int page,
        int pageSize,
        string? userId,
        string title);

    Task<PagedResponse<ModelTodoTask>> GetAllTasksByCreateDateAsync(
        int page,
        int pageSize,
        string? userId,
        DateTime createDate);

    Task<PagedResponse<ModelTodoTask>> GetAllTasksByDueDateAsync(
        int page,
        int pageSize,
        string? userId,
        DateTime dueDate);

    Task<PagedResponse<ModelTodoTask>> GetAllTasksByDateRangeAsync(
        int page,
        int pageSize,
        string? userId,
        DateTime fromDate,
        DateTime toDate);

    Task UpdateTaskAsync(int id, UpdateTodoTask item, string userId);

    Task DeleteTaskAsync(int id, string userId);
}
