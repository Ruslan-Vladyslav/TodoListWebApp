using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoComment;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Database.Services;

public class TodoTaskDatabaseService : ITodoTaskService
{
    private readonly TodoListDbContext _context;
    private readonly IAccessService _accessService;
    private readonly INotificationService _notificationService;

    public TodoTaskDatabaseService(
        TodoListDbContext context,
        IAccessService accessService,
        INotificationService notificationService)
    {
        this._context = context;
        this._accessService = accessService;
        this._notificationService = notificationService;
    }

    public async Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var role = await this._accessService.GetUserRoleAsync(
            item.UserId!,
            item.TodoListId);

        if (role == TodoListRole.Viewer)
        {
            throw new UnauthorizedAccessException();
        }

        var entity = new TodoTaskEntity
        {
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate,
            CreateDate = DateTime.UtcNow,
            CreatedByUserId = item.UserId,
            AssignedToUserId = item.AssignedUserId,
            AssignedByUserId = item.UserId,
            AssignedAt =
                item.AssignedUserId != null
                ? DateTime.UtcNow
                : null,
            Status = item.Status,
            TodoListId = item.TodoListId,
        };

        await _context.TodoTasks.AddAsync(entity);
        await _context.SaveChangesAsync();

        if (item.AssignedUserId != null &&
            item.AssignedUserId != item.UserId)
        {
            _ = await this._notificationService.CreateAsync(
                NotificationFactory.TaskAssigned(
                item.AssignedUserId,
                item.Title!,
                entity.Id));
        }

        return Map(entity);
    }

    public async Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId, string userId)
    {
        var role = await this._accessService.GetUserRoleAsync(userId, todoListId);

        if (role == TodoListRole.Viewer)
        {
            throw new UnauthorizedAccessException();
        }

        var tasks = await this._context.TodoTasks
            .AsNoTracking()
            .Where(t => t.TodoListId == todoListId)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .ToListAsync();

        return tasks.Select(Map);
    }

    public async Task<ModelTodoTask?> GetByIdTaskAsync(int id, string userId)
    {
        var entity = await this._context.TodoTasks
            .AsNoTracking()
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (entity == null)
        {
            throw new KeyNotFoundException();
        }

        var role = await this._accessService.GetUserRoleAsync(userId, entity.TodoListId);
        return Map(entity);
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksAsync(
        int page,
        int pageSize,
        int? todoListId,
        string? userId,
        TodoTaskStatus? status,
        string? sort)
    {
        IQueryable<TodoTaskEntity> query = this._context.TodoTasks
            .AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t =>
                this._context.TodoLists.Any(l =>
                    l.Id == t.TodoListId &&
                    (l.UserId == userId ||
                        this._context.TodoListAccesses.Any(a =>
                        a.TodoListId == l.Id &&
                        a.TargetUserId == userId &&
                        a.Accepted))));
        }

        if (todoListId.HasValue)
        {
            query = query.Where(t => t.TodoListId == todoListId);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, sort);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoTask>
        {
            Items = items.Select(Map),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task UpdateTaskAsync(
        int id,
        UpdateTodoTask item,
        string userId)
    {
        ArgumentNullException.ThrowIfNull(item);

        var entity = await this._context.TodoTasks
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException();

        var role = await this._accessService.GetUserRoleAsync(
            userId,
            entity.TodoListId);

        if (role == TodoListRole.Viewer)
        {
            throw new UnauthorizedAccessException();
        }

        var oldStatus = entity.Status;

        entity.Title = item.Title!;
        entity.Description = item.Description;
        entity.DueDate = item.DueDate;

        if (role == TodoListRole.Owner ||
            role == TodoListRole.Editor)
        {
            entity.Status = item.Status;
        }

        var isReassigning = item.AssignedUserId != entity.AssignedToUserId;

        if (isReassigning && role != TodoListRole.Owner)
        {
            throw new UnauthorizedAccessException("Only owner can reassign task");
        }

        if (role == TodoListRole.Owner &&
            item.AssignedUserId != entity.AssignedToUserId)
        {
            entity.AssignedToUserId = item.AssignedUserId;
            entity.AssignedByUserId = userId;
            entity.AssignedAt = DateTime.UtcNow;

            if (item.AssignedUserId != null)
            {
                _ = await this._notificationService.CreateAsync(
                    NotificationFactory.TaskAssigned(
                        item.AssignedUserId,
                        entity.Title,
                        entity.Id));
            }
        }

        await _context.SaveChangesAsync();

        if (oldStatus != TodoTaskStatus.Completed &&
            entity.Status == TodoTaskStatus.Completed)
        {
            var ownerId = await this._context.TodoLists
                .Where(x => x.Id == entity.TodoListId)
                .Select(x => x.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(ownerId) && ownerId != userId)
            {
                _ = await this._notificationService.CreateAsync(
                    NotificationFactory.TaskCompleted(
                        ownerId,
                        entity.Title,
                        entity.Id));
            }
        }
    }

    public async Task DeleteTaskAsync(int id, string userId)
    {
        var entity = await this._context.TodoTasks.FindAsync(id)
            ?? throw new KeyNotFoundException();

        var role = await this._accessService.GetUserRoleAsync(userId, entity.TodoListId);

        if (role != TodoListRole.Owner)
        {
            throw new UnauthorizedAccessException();
        }

        _context.TodoTasks.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByCreateDateAsync(
        int page, int pageSize, string? userId, DateTime createDate)
    {
        var query = ApplyUserAccess(
            _context.TodoTasks.AsNoTracking(),
            userId);

        query = query.Where(t => t.CreateDate.Date == createDate.Date);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.CreateDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoTask>
        {
            Items = items.Select(Map),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByDueDateAsync(
        int page, int pageSize, string? userId, DateTime dueDate)
    {
        var query = ApplyUserAccess(
            _context.TodoTasks.AsNoTracking(),
            userId);

        query = query.Where(t => t.DueDate.Date == dueDate.Date);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoTask>
        {
            Items = items.Select(Map),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByDateRangeAsync(
        int page, int pageSize, string? userId, DateTime fromDate, DateTime toDate)
    {
        var query = ApplyUserAccess(
            _context.TodoTasks.AsNoTracking(),
            userId);

        query = query.Where(t =>
            t.DueDate >= fromDate.Date &&
            t.DueDate <= toDate.Date);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoTask>
        {
            Items = items.Select(Map),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByTitleAsync(
        int page, int pageSize, string? userId, string title)
    {
        var query = ApplyUserAccess(
            _context.TodoTasks.AsNoTracking(),
            userId);

        query = query.Where(t => t.Title.Contains(title));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoTask>
        {
            Items = items.Select(Map),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static IQueryable<TodoTaskEntity> ApplySorting(
        IQueryable<TodoTaskEntity> query,
        string? sort)
    {
        if (string.IsNullOrEmpty(sort))
        {
            return query.OrderBy(x => x.DueDate);
        }

        var parts = sort.Split('(');
        var order = parts[0].ToLower();
        var field = parts.Length > 1
            ? parts[1].TrimEnd(')').ToLower()
            : string.Empty;

        return order switch
        {
            "asc" => field switch
            {
                "title" => query.OrderBy(x => x.Title),
                "status" => query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.DueDate)
            },

            "desc" => field switch
            {
                "title" => query.OrderByDescending(x => x.Title),
                "status" => query.OrderByDescending(x => x.Status),
                _ => query.OrderByDescending(x => x.DueDate)
            },

            _ => query.OrderBy(x => x.DueDate)
        };
    }

    private static ModelTodoTask Map(TodoTaskEntity e)
    {
        return new ModelTodoTask
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CreateDate = e.CreateDate,
            UserId = e.CreatedByUserId,
            AssignedUserId = e.AssignedToUserId,
            Status = e.Status,
            TodoListId = e.TodoListId,

            Tags = e.Tags != null
                ? e.Tags.Select(t => new ModelTodoTag
                {
                    Id = t.Id,
                    Name = t.Name,
                }).ToList()
                : new List<ModelTodoTag>(),

            Comments = e.Comments != null
                ? e.Comments.Select(c => new ModelTodoComment
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreateDate = c.CreateDate,
                    UserId = c.UserId,
                }).ToList()
                : new List<ModelTodoComment>(),
        };
    }

    private IQueryable<TodoTaskEntity> ApplyUserAccess(
        IQueryable<TodoTaskEntity> query,
        string? userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return query;
        }

        return query.Where(t =>
            this._context.TodoLists.Any(l =>
                l.Id == t.TodoListId &&
                (l.UserId == userId ||
                    this._context.TodoListAccesses.Any(a =>
                        a.TodoListId == l.Id &&
                        a.TargetUserId == userId &&
                        a.Accepted))));
    }
}
