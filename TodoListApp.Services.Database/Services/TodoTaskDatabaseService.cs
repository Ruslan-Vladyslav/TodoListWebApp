using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Database.Services;

public class TodoTaskDatabaseService : ITodoTaskService
{
    private readonly TodoListDbContext _context;
    private readonly IAccessService _accessService;

    public TodoTaskDatabaseService(
        TodoListDbContext context,
        IAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var entity = new TodoTaskEntity
        {
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate,
            CreateDate = DateTime.UtcNow,
            CreatedByUserId = item.UserId,
            AssignedToUserId = item.AssignedUserId,
            AssignedByUserId = item.UserId,
            AssignedAt = item.AssignedUserId != null ? DateTime.UtcNow : null,
            Status = item.Status,
            TodoListId = item.TodoListId,
        };

        await _context.TodoTasks.AddAsync(entity);
        await _context.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId, string userId)
    {
        if (!await _accessService.CanViewListAsync(userId, todoListId))
        {
            throw new UnauthorizedAccessException();
        }

        var tasks = await _context.TodoTasks
            .AsNoTracking()
            .Where(t => t.TodoListId == todoListId)
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .ToListAsync();

        return tasks.Select(Map);
    }

    public async Task<ModelTodoTask?> GetByIdTaskAsync(int id, string userId)
    {
        var entity = await _context.TodoTasks
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException();

        if (!await _accessService.CanViewListAsync(userId, entity.TodoListId))
        {
            throw new UnauthorizedAccessException();
        }

        return Map(entity);
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksAsync(
        int page,
        int pageSize,
        int? todoListId,
        string? userId,
        TodoTaskStatus? status,
        string? sort)
    {
        IQueryable<TodoTaskEntity> query = _context.TodoTasks.AsNoTracking();

        if (todoListId.HasValue)
        {
            query = query.Where(t => t.TodoListId == todoListId);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.CreatedByUserId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status);
        }

        query = ApplySorting(query, sort);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(Map);
    }

    public async Task UpdateTaskAsync(int id, UpdateTodoTask item, string userId)
    {
        ArgumentNullException.ThrowIfNull(item);

        var entity = await _context.TodoTasks
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException();

        if (!await _accessService.CanEditListAsync(userId, entity.TodoListId))
        {
            throw new UnauthorizedAccessException();
        }

        entity.Title = item.Title!;
        entity.Description = item.Description;
        entity.DueDate = item.DueDate;
        entity.Status = item.Status;

        if (item.AssignedUserId != entity.AssignedToUserId)
        {
            entity.AssignedToUserId = item.AssignedUserId;
            entity.AssignedByUserId = userId;
            entity.AssignedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int id, string userId)
    {
        var entity = await _context.TodoTasks.FindAsync(id)
            ?? throw new KeyNotFoundException();

        if (!await _accessService.CanEditListAsync(userId, entity.TodoListId))
        {
            throw new UnauthorizedAccessException();
        }

        _context.TodoTasks.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByCreateDateAsync(
        int page, int pageSize, string? userId, DateTime createDate)
    {
        return await FilterByDate(t => t.CreateDate, page, pageSize, userId, createDate);
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByDueDateAsync(
        int page, int pageSize, string? userId, DateTime dueDate)
    {
        return await FilterByDate(t => t.DueDate, page, pageSize, userId, dueDate);
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByDateRangeAsync(
        int page, int pageSize, string? userId, DateTime fromDate, DateTime toDate)
    {
        var query = _context.TodoTasks.AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.CreatedByUserId == userId);
        }

        query = query.Where(t =>
            t.DueDate >= fromDate.Date &&
            t.DueDate <= toDate.Date);

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(Map);
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByTitleAsync(
        int page, int pageSize, string? userId, string title)
    {
        var query = _context.TodoTasks.AsNoTracking();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.CreatedByUserId == userId);
        }

        query = query.Where(t => t.Title.Contains(title));

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(Map);
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
            : "";

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

    private static async Task<IEnumerable<ModelTodoTask>> FilterByDate(
        Func<TodoTaskEntity, DateTime> selector,
        int page,
        int pageSize,
        string? userId,
        DateTime date)
    {
        throw new NotImplementedException("Use EF query version instead (optimization required)");
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
                    Name = t.Name
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
                : new List<ModelTodoComment>()
        };
    }
}
