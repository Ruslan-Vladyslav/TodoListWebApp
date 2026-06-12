using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entity;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Services.Database.Services;

public class TodoListDatabaseService : ITodoListService
{
    private readonly INotificationService _notificationService;
    private readonly TodoListDbContext todoListContext;
    private readonly IUserService _userService;

    public TodoListDatabaseService(TodoListDbContext context, INotificationService notificationService, IUserService userService)
    {
        this.todoListContext = context;
        this._notificationService = notificationService;
        this._userService = userService;

    }

    public async Task<ModelTodoList> CreateListAsync(CreateTodoList item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "TodoList item cannot be null.");
        }

        var model = new TodoListEntity
        {
            Description = item.Description,
            Title = item.Title!,
            UserId = item.UserId,
        };

        var createdEntity = await this.todoListContext.TodoLists.AddAsync(model);
        _ = await this.todoListContext.SaveChangesAsync();

        return new ModelTodoList
        {
            Id = createdEntity.Entity.Id,
            Title = createdEntity.Entity.Title,
            Description = createdEntity.Entity.Description,
            UserId = createdEntity.Entity.UserId,
        };
    }

    public async Task DeleteListAsync(int id)
    {
        var entity = await this.todoListContext.TodoLists
            .Include(x => x.Accesses)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new NotFoundException(
                $"TodoList with id {id} not found.");

        var users = entity.Accesses
            .Where(x => x.Accepted)
            .Select(x => x.TargetUserId)
            .ToList();

        foreach (var userId in users)
        {
            _ = await this._notificationService.CreateAsync(
                NotificationFactory.ListDeleted(
                    userId,
                    entity.Title,
                    entity.Id));
        }

        todoListContext.TodoLists.Remove(entity);
        await todoListContext.SaveChangesAsync();
    }

    public async Task<PagedResponse<ModelTodoList>> GetAllListAsync(int page, int pageSize)
    {
        var query = this.todoListContext.TodoLists
            .Include(x => x.Accesses)
            .Include(x => x.TodoTasks);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var models = new List<ModelTodoList>();

        foreach (var e in items)
        {
            var userName = await _userService.GetUserNameAsync(e.UserId!);

            models.Add(new ModelTodoList
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                UserId = e.UserId,
                UserName = userName,
                TaskCount = e.TodoTasks.Count,
                MemberCount = e.Accesses.Count(a => a.Accepted) + 1,
            });
        }

        return new PagedResponse<ModelTodoList>
        {
            Items = models,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<PagedResponse<ModelTodoList>> GetAllListByUserAsync(int page, int pageSize, string userId)
    {
        var query = this.todoListContext.TodoLists
             .Where(l =>
                l.UserId == userId ||
                l.Accesses.Any(a => a.TargetUserId == userId && a.Accepted));

        var totalCount = await query.CountAsync();

        var pagedQuery = query
            .OrderBy(t => t.Title)
            .Select(t => new ModelTodoList
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                UserId = t.UserId,
                TaskCount = t.TodoTasks.Count(),
                MemberCount = t.Accesses.Count(a => a.Accepted) + 1,
            });

        var paged = await pagedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoList>
        {
            Items = paged,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ModelTodoList?> GetByIdListAsync(int id)
    {
        var entity = await this.todoListContext.TodoLists
           .Include(x => x.TodoTasks)
           .Include(x => x.Accesses)
           .FirstOrDefaultAsync(x => x.Id == id)
           ?? throw new NotFoundException($"TodoList with id {id} not found.");


        var userName = await _userService.GetUserNameAsync(entity.UserId!);
        Console.WriteLine($"UserName for UserId {entity.UserId}: {userName}");
        return new ModelTodoList
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            UserName = userName,
            UserId = entity.UserId,
            TaskCount = entity.TodoTasks.Count,
            MemberCount = entity.Accesses.Count(a => a.Accepted) + 1,
        };
    }

    public async Task UpdateListAsync(int id, UpdateTodoList item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "TodoList item cannot be null.");
        }

        var entity = await this.todoListContext.TodoLists.FindAsync(id)
            ?? throw new NotFoundException($"TodoList with id {id} not found.");

        entity.Title = item.Title!;
        entity.Description = item.Description;

        _ = this.todoListContext.TodoLists.Update(entity);
        _ = await this.todoListContext.SaveChangesAsync();
    }
}
