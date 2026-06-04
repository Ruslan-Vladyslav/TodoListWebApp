using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.User;

namespace TodoListApp.Services.Services;

public class UserDatabaseService : IUserService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly Dictionary<string, string?> _cache = new();

    public UserDatabaseService(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserModel?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return null;
        }

        return new UserModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email,
        };
    }

    public async Task<List<UserModel>> GetAllAsync()
    {
        return await _userManager.Users
            .Select(x => new UserModel
            {
                Id = x.Id,
                UserName = x.UserName!,
                Email = x.Email,
            })
            .ToListAsync();
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        if (_cache.TryGetValue(userId, out var name))
        {
            return name;
        }

        var user = await GetByIdAsync(userId);

        _cache[userId] = user?.UserName;

        return user?.UserName;
    }

    public async Task<UserModel?> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return null;
        }

        return new UserModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email
        };
    }

    public async Task<Dictionary<string, string?>> GetUsersByIdsAsync(List<string> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            return new Dictionary<string, string?>();
        }

        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.UserName })
            .ToListAsync();

        return users.ToDictionary(
            x => x.Id,
            x => x.UserName
        );
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _userManager.Users
            .AnyAsync(x => x.UserName == userName);
    }

    public async Task UpdateUserNameAsync(
        string userId,
        string userName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        user.UserName = userName;

        await _userManager.UpdateAsync(user);
    }

    public async Task DeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
    }
}
