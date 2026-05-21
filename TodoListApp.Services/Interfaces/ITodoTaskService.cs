using TodoListApp.Services.Enums;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Interfaces;

public interface ITodoTaskService
{
    Task<ModelTodoTask?> GetByIdTaskAsync(int id);

    Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item);

    Task UpdateTaskAsync(int id, UpdateTodoTask item);

    Task DeleteTaskAsync(int id);

    Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId);

    Task<IEnumerable<ModelTodoTask>> GetAllTasksAsync(int page, int pageSize, int? toDoListId, string? userId, TodoTaskStatus? status, string? sort);

    Task<IEnumerable<ModelTodoTask>> GetAllTasksByTitleAsync(int page, int pageSize, string? userId, string title);

    Task<IEnumerable<ModelTodoTask>> GetAllTasksByCreateDateAsync(int page, int pageSize, string? userId, DateTime createDate);

    Task<IEnumerable<ModelTodoTask>> GetAllTasksByDueDateAsync(int page, int pageSize, string? userId, DateTime dueDate);

    Task<IEnumerable<ModelTodoTask>> GetAllTasksByDateRangeAsync(int page, int pageSize, string? userId, DateTime fromDate, DateTime toDate);
}
