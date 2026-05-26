using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.Services.Database.Services;

public class AccessDatabaseService : IAccessService
{
    private readonly TodoListDbContext _context;

    public AccessDatabaseService(TodoListDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanViewListAsync(string userId, int listId)
    {
        return await _context.TodoLists.AnyAsync(l =>
            l.Id == listId &&
            (
                l.UserId == userId ||
                _context.TodoListAccesses.Any(a =>
                    a.TodoListId == listId &&
                    a.TargetUserId == userId)
            ));
    }

    public async Task<bool> CanEditListAsync(string userId, int listId)
    {
        return await _context.TodoLists.AnyAsync(l =>
            l.Id == listId &&
            (
                l.UserId == userId ||
                _context.TodoListAccesses.Any(a =>
                    a.TodoListId == listId &&
                    a.TargetUserId == userId &&
                    a.Role == TodoListRole.Editor)
            ));
    }

    public async Task GrantAccessAsync(string ownerUserId, string userId, int listId, TodoListRole role)
    {
        var exists = await _context.TodoListAccesses.AnyAsync(x =>
            x.TodoListId == listId &&
            x.TargetUserId == userId);

        if (exists)
        {
            return;
        }

        var entity = new TodoListAccessEntity
        {
            TodoListId = listId,
            OwnerUserId = ownerUserId,
            TargetUserId = userId,
            Role = role,
            SharedAt = DateTime.UtcNow
        };

        _context.TodoListAccesses.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAccessAsync(string userId, int listId)
    {
        var access = await _context.TodoListAccesses.FirstOrDefaultAsync(x =>
            x.TodoListId == listId &&
            x.TargetUserId == userId);

        if (access == null)
        {
            return;
        }

        _context.TodoListAccesses.Remove(access);
        await _context.SaveChangesAsync();
    }
}
