using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Database.Services;

public class TodoTagDatabaseService : ITodoTagService
{
    private readonly TodoListDbContext _context;

    public TodoTagDatabaseService(TodoListDbContext context)
    {
        this._context = context;
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId)
    {
        var task = await _context.TodoTasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

        var tag = await _context.TodoTags.FindAsync(tagId)
            ?? throw new KeyNotFoundException($"Tag {tagId} not found");

        if (task.Tags?.Any(t => t.Id == tagId) == true)
        {
            return;
        }

        task.Tags ??= new List<TodoTagEntity>();
        task.Tags.Add(tag);

        await _context.SaveChangesAsync();
    }

    public async Task<ModelTodoTag> CreateTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be empty.");
        }

        var normalized = tagName.Trim().ToLower();

        var existingTag = await _context.TodoTags
            .FirstOrDefaultAsync(t => t.Name!.ToLower() == normalized);

        if (existingTag != null)
        {
            return new ModelTodoTag
            {
                Id = existingTag.Id,
                Name = existingTag.Name
            };
        }

        var entity = new TodoTagEntity
        {
            Name = tagName.Trim()
        };

        await _context.TodoTags.AddAsync(entity);
        await _context.SaveChangesAsync();

        return new ModelTodoTag
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public async Task DeleteTagFromTaskAsync(int taskId, int tagId)
    {
        var task = await _context.TodoTasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

        var tagInTask = task.Tags.FirstOrDefault(t => t.Id == tagId);

        if (tagInTask == null)
        {
            return;
        }

        task.Tags.Remove(tagInTask);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelTodoTag>> GetAllTagsAsync(int page, int pageSize)
    {
        var tags = await _context.TodoTags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return tags.Select(t => new ModelTodoTag
        {
            Id = t.Id,
            Name = t.Name
        });
    }

    public async Task<ModelTodoTag?> GetByIdTagAsync(int id)
    {
        var entity = await _context.TodoTags
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException($"Tag {id} not found");

        return new ModelTodoTag
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public async Task<IEnumerable<ModelTodoTask>> GetTasksByTagAsync(int tagId)
    {
        var tasks = await _context.TodoTasks
            .AsNoTracking()
            .Include(t => t.Tags)
            .Include(t => t.Comments)
            .Where(t => t.Tags.Any(tag => tag.Id == tagId))
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
