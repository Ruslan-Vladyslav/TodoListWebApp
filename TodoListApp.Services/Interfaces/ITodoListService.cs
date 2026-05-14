using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Services.Interfaces;

public interface ITodoListService
{
    Task<IEnumerable<ModelTodoList>> GetAllListAsync(int page, int pageSize);

    Task<IEnumerable<ModelTodoList>> GetAllListByUserAsync(int page, int pageSize, string userId);

    Task<ModelTodoList?> GetByIdListAsync(int id);

    Task<ModelTodoList> CreateListAsync(CreateTodoList item);

    Task UpdateListAsync(int id, UpdateTodoList item);

    Task DeleteListAsync(int id);
}
