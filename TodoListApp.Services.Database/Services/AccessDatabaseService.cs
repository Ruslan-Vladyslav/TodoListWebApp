using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Access;

namespace TodoListApp.Services.Database.Services;

public class AccessDatabaseService : IAccessService
{
    private readonly TodoListDbContext _context;

    public AccessDatabaseService(TodoListDbContext context)
    {
        this._context = context;
    }

    public async Task GrantAccessAsync(
        string ownerUserId,
        string userId,
        int listId,
        TodoListRole role)
    {
        var list = await this._context.TodoLists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == listId);

        if (list == null)
        {
            throw new KeyNotFoundException();
        }

        if (list.UserId != ownerUserId)
        {
            throw new UnauthorizedAccessException();
        }

        var exists = await this._context.TodoListAccesses.AnyAsync(x =>
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
            Accepted = true,
            AcceptedAt = DateTime.UtcNow,
        };

        this._context.TodoListAccesses.Add(entity);
        await this._context.SaveChangesAsync();
    }

    public async Task RevokeAccessAsync(
        string ownerUserId,
        string targetUserId,
        int listId)
    {
        var list = await this._context.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == listId)
            ?? throw new KeyNotFoundException();

        if (list.UserId != ownerUserId)
        {
            throw new UnauthorizedAccessException();
        }

        if (targetUserId == list.UserId)
        {
            throw new InvalidOperationException("Owner cannot be removed");
        }

        var access = await this._context.TodoListAccesses
            .SingleOrDefaultAsync(x =>
                x.TodoListId == listId &&
                x.TargetUserId == targetUserId);

        if (access == null)
        {
            return;
        }

        _context.TodoListAccesses.Remove(access);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelTodoListAccess>> GetAccessListAsync(int listId)
    {
        var items = await this._context.TodoListAccesses
            .Where(x => x.TodoListId == listId && x.Accepted)
            .ToListAsync();

        return items.Select(x => new ModelTodoListAccess
        {
            TodoListId = x.TodoListId,
            OwnerUserId = x.OwnerUserId,
            TargetUserId = x.TargetUserId,
            TargetUserName = x.TargetUserName,
            Role = x.Role,
            Accepted = x.Accepted,
        });
    }

    public async Task<TodoListRole> GetUserRoleAsync(string userId, int listId)
    {
        var list = await this._context.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == listId);

        if (list == null)
        {
            return TodoListRole.Viewer;
        }

        if (list.UserId == userId)
        {
            return TodoListRole.Owner;
        }

        var access = await this._context.TodoListAccesses
            .AsNoTracking()
            .Where(x =>
                x.TodoListId == listId &&
                x.TargetUserId == userId &&
                x.Accepted)
            .FirstOrDefaultAsync();

        return access?.Role ?? TodoListRole.Viewer;
    }
}
