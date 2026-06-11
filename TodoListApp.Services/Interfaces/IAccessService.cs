using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Access;

namespace TodoListApp.Services.Interfaces;

public interface IAccessService
{
    Task GrantAccessAsync(string ownerUserId, string userId, int listId, TodoListRole role);

    Task RevokeAccessAsync(string ownerUserId, string targetUserId, int listId);

    Task<IEnumerable<ModelTodoListAccess>> GetAccessListAsync(int listId);

    Task<TodoListRole?> GetUserRoleAsync(string userId, int listId);

    Task UpdateRoleAsync(string ownerUserId, string targetUserId, int listId, TodoListRole newRole);
}
