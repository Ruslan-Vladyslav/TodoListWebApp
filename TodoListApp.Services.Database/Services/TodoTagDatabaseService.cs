using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Database.Services;

public class TodoTagDatabaseService : ITodoTagService
{
    private readonly TodoListDbContext _context;
    private readonly IAccessService _accessService;

    public TodoTagDatabaseService(TodoListDbContext context, IAccessService accessService)
    {
        this._context = context;
        this._accessService = accessService;
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId, string userId)
    {
        var task = await this._context.TodoTasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

        var role = await this._accessService.GetUserRoleAsync(userId, task.TodoListId);

        if (role == TodoListRole.Viewer)
        {
            throw new UnauthorizedAccessException();
        }

        var tag = await this._context.TodoTags.FindAsync(tagId)
            ?? throw new KeyNotFoundException($"Tag {tagId} not found");

        if (task.Tags!.Any(t => t.Id == tagId))
        {
            return;
        }

        task.Tags!.Add(tag);

        await _context.SaveChangesAsync();
    }

    public async Task<ModelTodoTag> CreateTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be empty.");
        }

        var normalized = tagName.Trim().ToLower();

        var existingTag = await this._context.TodoTags
            .FirstOrDefaultAsync(t => t.Name!.ToLower() == normalized);

        if (existingTag != null)
        {
            return new ModelTodoTag
            {
                Id = existingTag.Id,
                Name = existingTag.Name,
            };
        }

        var entity = new TodoTagEntity
        {
            Name = tagName.Trim()!
        };

        await _context.TodoTags.AddAsync(entity);
        await _context.SaveChangesAsync();

        return new ModelTodoTag
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public async Task DeleteTagFromTaskAsync(int taskId, int tagId, string userId)
    {
        var task = await this._context.TodoTasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

        var role = await this._accessService.GetUserRoleAsync(userId, task.TodoListId);

        if (role == TodoListRole.Viewer)
        {
            throw new UnauthorizedAccessException();
        }

        var tagInTask = task.Tags!.FirstOrDefault(t => t.Id == tagId);

        if (tagInTask == null)
        {
            return;
        }

        task.Tags.Remove(tagInTask);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResponse<ModelTodoTag>> GetAllTagsAsync(int page, int pageSize)
    {
        var query = this._context.TodoTags.AsNoTracking();

        var totalCount = await query.CountAsync();

        var tags = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ModelTodoTag>
        {
            Items = tags.Select(t => new ModelTodoTag
            {
                Id = t.Id,
                Name = t.Name,
            }),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ModelTodoTag?> GetByIdTagAsync(int id)
    {
        var entity = await this._context.TodoTags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Tag {id} not found");

        return new ModelTodoTag
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public async Task<IEnumerable<ModelTodoTask>> GetTasksByTagAsync(int tagId, string userId)
    {
        var tasks = await this._context.TodoTasks
            .AsNoTracking()
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .Where(t => t.Tags!.Any(tag => tag.Id == tagId))
            .Where(t =>
                this._context.TodoLists.Any(l =>
                    l.Id == t.TodoListId &&
                    (
                        l.UserId == userId ||
                        this._context.TodoListAccesses.Any(a =>
                            a.TodoListId == l.Id &&
                            a.TargetUserId == userId &&
                            a.Accepted))))
            .ToListAsync();

        return tasks.Select(task => new ModelTodoTask
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            CreateDate = task.CreateDate,
            UserId = task.CreatedByUserId,
            AssignedUserId = task.AssignedToUserId,
            Status = task.Status,
            TodoListId = task.TodoListId,
        });
    }
}
