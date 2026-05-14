using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entity;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Services.Database.Services;

public class TodoListDatabaseService : ITodoListService
{
    private readonly TodoListDbContext todoListContext;

    public TodoListDatabaseService(TodoListDbContext context)
    {
        this.todoListContext = context;
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
        var entity = await this.todoListContext.TodoLists.FindAsync(id)
            ?? throw new NotFoundException($"TodoList with id {id} not found.");

        _ = this.todoListContext.TodoLists.Remove(entity);
        _ = await this.todoListContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelTodoList>> GetAllListAsync(int page, int pageSize)
    {
        var items = await this.todoListContext.TodoLists
            .OrderBy(t => t.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(e => new ModelTodoList
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            UserId = e.UserId,
        });
    }

    public async Task<IEnumerable<ModelTodoList>> GetAllListByUserAsync(int page, int pageSize, string userId)
    {
        var items = await this.todoListContext.TodoLists
             .Where(t => t.UserId == userId)
             .OrderBy(t => t.Title)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();

        return items.Select(e => new ModelTodoList
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            UserId = e.UserId,
        });
    }

    public async Task<ModelTodoList?> GetByIdListAsync(int id)
    {
        var entity = await this.todoListContext.TodoLists.FindAsync(id) ?? throw new NotFoundException($"TodoList with id {id} not found.");
        return new ModelTodoList
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            UserId = entity.UserId,
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
        entity.UserId = item.UserId;

        _ = this.todoListContext.TodoLists.Update(entity);
        _ = await this.todoListContext.SaveChangesAsync();
    }
}
