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

        using var transaction = await this._context.Database.BeginTransactionAsync();

        try
        {
            var tasksToReassign = await this._context.TodoTasks
                .Where(t => t.TodoListId == listId && t.AssignedToUserId == targetUserId)
                .ToListAsync();

            foreach (var task in tasksToReassign)
            {
                task.AssignedToUserId = ownerUserId;
            }

            _context.TodoListAccesses.Remove(access);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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

    public async Task<TodoListRole?> GetUserRoleAsync(string userId, int listId)
    {
        var list = await this._context.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == listId);

        if (list == null)
        {
            return null;
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

        if (access == null)
        {
            return null;
        }

        return access.Role;
    }

    public async Task UpdateRoleAsync(
        string ownerUserId,
        string targetUserId,
        int listId,
        TodoListRole newRole)
    {
        var list = await this._context.TodoLists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == listId)
            ?? throw new KeyNotFoundException();

        if (list.UserId != ownerUserId)
        {
            throw new UnauthorizedAccessException();
        }

        if (targetUserId == ownerUserId)
        {
            throw new InvalidOperationException("Cannot change the owner's role.");
        }

        var access = await this._context.TodoListAccesses
            .FirstOrDefaultAsync(x =>
                x.TodoListId == listId &&
                x.TargetUserId == targetUserId &&
                x.Accepted)
            ?? throw new KeyNotFoundException("User does not have access to this list.");

        access.Role = newRole;
        await this._context.SaveChangesAsync();
    }
}
