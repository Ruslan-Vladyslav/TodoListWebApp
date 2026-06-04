using TodoListApp.WebApi.Models.Models.User;

namespace TodoListApp.Services.Interfaces;

public interface IUserService
{
    Task<UserModel?> GetByIdAsync(string id);

    Task<List<UserModel>> GetAllAsync();

    Task<string?> GetUserNameAsync(string userId);

    Task<UserModel?> GetByEmailAsync(string email);

    Task<Dictionary<string, string?>> GetUsersByIdsAsync(List<string> ids);

    Task UpdateUserNameAsync(string userId, string userName);

    Task DeleteAsync(string userId);

    Task<bool> UserNameExistsAsync(string userName);
}
