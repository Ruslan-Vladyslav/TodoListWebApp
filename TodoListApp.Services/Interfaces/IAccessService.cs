using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.Services.Interfaces;

public interface IAccessService
{
    Task<bool> CanViewListAsync(string userId, int listId);

    Task<bool> CanEditListAsync(string userId, int listId);

    Task GrantAccessAsync(string userId, int listId, TodoListRole role);

    Task RevokeAccessAsync(string userId, int listId);
}
