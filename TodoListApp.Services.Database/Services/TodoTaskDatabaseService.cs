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
    private readonly TodoListDbContext _todoListContext;

    public TodoTaskDatabaseService(TodoListDbContext context)
    {
        this._todoListContext = context;
    }

    public async Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId)
    {
        return await this._todoListContext.TodoTasks
            .Where(t => t.TodoListId == todoListId)
            .Select(t => new ModelTodoTask
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                DueDate = t.DueDate,
                TodoListId = t.TodoListId
            })
            .ToListAsync();
    }

    public async Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "TodoTask cannot be null.");
        }

        var entity = new TodoTaskEntity
        {
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate,
            CreateDate = DateTime.Now,
            UserId = item.UserId,
            AssignedUserId = item.AssignedUserId,
            Status = item.Status,
            TodoListId = item.TodoListId
        };

        var created = await this._todoListContext.TodoTasks.AddAsync(entity);
        _ = await this._todoListContext.SaveChangesAsync();

        return new ModelTodoTask
        {
            Id = created.Entity.Id,
            Title = created.Entity.Title,
            Description = created.Entity.Description,
            DueDate = created.Entity.DueDate,
            CreateDate = created.Entity.CreateDate,
            UserId = created.Entity.UserId!,
            AssignedUserId = created.Entity.AssignedUserId,
            Status = created.Entity.Status,
            TodoListId = created.Entity.TodoListId
        };
    }

    public async Task DeleteTaskAsync(int id)
    {
        var entity = await this._todoListContext.TodoTasks.FindAsync(id)
                      ?? throw new KeyNotFoundException($"TodoTask with id {id} not found.");

        _ = this._todoListContext.TodoTasks.Remove(entity);
        _ = await this._todoListContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksAsync(int page, int pageSize, int? toDoListId, string? userId, TodoTaskStatus? status, string? sort)
    {
        IQueryable<TodoTaskEntity> query = this._todoListContext.TodoTasks.AsQueryable();

        if (toDoListId.HasValue)
        {
            query = query.Where(t => t.TodoListId == toDoListId.Value);
        }
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }
        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (!string.IsNullOrEmpty(sort))
        {
            var parts = sort.Split('(');
            var order = parts[0].ToLower(System.Globalization.CultureInfo.CurrentCulture).Trim();
            var field = parts.Length > 1 ? parts[1].TrimEnd(')').ToLower(System.Globalization.CultureInfo.CurrentCulture) : string.Empty;

            if (order == "asc")
            {
                query = field switch
                {
                    "title" => query.OrderBy(t => t.Title),
                    "duedate" => query.OrderBy(t => t.DueDate),
                    "status" => query.OrderBy(t => t.Status),
                    _ => query.OrderBy(t => t.DueDate)
                };
            }
            else if (order == "desc")
            {
                query = field switch
                {
                    "title" => query.OrderByDescending(t => t.Title),
                    "duedate" => query.OrderByDescending(t => t.DueDate),
                    "status" => query.OrderByDescending(t => t.Status),
                    _ => query.OrderByDescending(t => t.DueDate)
                };
            }
        }
        else
        {
            query = query.OrderBy(t => t.DueDate);
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(e => new ModelTodoTask
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CreateDate = e.CreateDate,
            UserId = e.UserId!,
            AssignedUserId = e.AssignedUserId,
            Status = e.Status,
            TodoListId = e.TodoListId,
            Tags = e.Tags!.Select(t => new ModelTodoTag { Id = t.Id, Name = t.Name }).ToList(),
            Comments = e.Comments!.Select(c => new ModelTodoComment { Id = c.Id, UserId = c.UserId, Text = c.Text, CreateDate = c.CreateDate }).ToList()
        });
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByCreateDateAsync(int page, int pageSize, string? userId, DateTime createDate)
    {
        var query = this._todoListContext.TodoTasks.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var startDate = createDate.Date;
        var endDate = startDate.AddDays(1);

        query = query.Where(t => t.CreateDate >= startDate && t.CreateDate < endDate);

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(e => new ModelTodoTask
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CreateDate = e.CreateDate,
            UserId = e.UserId!,
            AssignedUserId = e.AssignedUserId,
            Status = e.Status,
            TodoListId = e.TodoListId
        });
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByDueDateAsync(int page, int pageSize, string? userId, DateTime dueDate)
    {
        var query = this._todoListContext.TodoTasks.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var startDate = dueDate.Date;
        var endDate = startDate.AddDays(1);

        query = query.Where(t => t.DueDate >= startDate && t.DueDate < endDate);

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(e => new ModelTodoTask
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CreateDate = e.CreateDate,
            UserId = e.UserId!,
            AssignedUserId = e.AssignedUserId,
            Status = e.Status,
            TodoListId = e.TodoListId
        });
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByDateRangeAsync(
        int page,
        int pageSize,
        string? userId,
        DateTime fromDate,
        DateTime toDate)
    {
        var query = this._todoListContext.TodoTasks.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var startDate = fromDate.Date;
        var endDate = toDate.Date.AddDays(1);

        query = query.Where(t =>
            t.DueDate >= startDate &&
            t.DueDate < endDate);

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(e => new ModelTodoTask
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CreateDate = e.CreateDate,
            UserId = e.UserId!,
            AssignedUserId = e.AssignedUserId,
            Status = e.Status,
            TodoListId = e.TodoListId
        });
    }

    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByTitleAsync(int page, int pageSize, string? userId, string title)
    {
        var query = this._todoListContext.TodoTasks.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(t => t.Title == title);
        }

        var items = await query
            .OrderBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(e => new ModelTodoTask
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            DueDate = e.DueDate,
            CreateDate = e.CreateDate,
            UserId = e.UserId!,
            AssignedUserId = e.AssignedUserId,
            Status = e.Status,
            TodoListId = e.TodoListId
        });
    }

    public async Task<ModelTodoTask?> GetByIdTaskAsync(int id)
    {
        var entity = await this._todoListContext.TodoTasks
         .Include(t => t.Tags)
         .Include(t => t.Comments)
         .FirstOrDefaultAsync(t => t.Id == id)
         ?? throw new NotFoundException($"TodoTask with id {id} not found.");

        return new ModelTodoTask
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            DueDate = entity.DueDate,
            CreateDate = entity.CreateDate,
            UserId = entity.UserId!,
            AssignedUserId = entity.AssignedUserId,
            Status = entity.Status,
            TodoListId = entity.TodoListId,
            Tags = entity.Tags!.Select(t => new ModelTodoTag
            {
                Id = t.Id,
                Name = t.Name
            }).ToList(),
            Comments = entity.Comments!.Select(c => new ModelTodoComment
            {
                Id = c.Id,
                Text = c.Text
            }).ToList()
        };
    }

    public async Task UpdateTaskAsync(int id, UpdateTodoTask item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var entity = await this._todoListContext.TodoTasks.FindAsync(id)
                     ?? throw new KeyNotFoundException($"TodoTask with id {id} not found.");

        entity.Title = item.Title!;
        entity.Description = item.Description;
        entity.DueDate = item.DueDate;
        entity.AssignedUserId = item.AssignedUserId;
        entity.Status = item.Status;
        entity.UserId = item.UserId;
        entity.TodoListId = item.TodoListId;

        _ = this._todoListContext.TodoTasks.Update(entity);
        _ = await this._todoListContext.SaveChangesAsync();
    }
}
