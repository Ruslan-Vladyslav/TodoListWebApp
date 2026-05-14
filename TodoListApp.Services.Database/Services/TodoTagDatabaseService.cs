using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.Database.Services;

public class TodoTagDatabaseService : ITodoTagService
{
    private readonly TodoListDbContext context;

    public TodoTagDatabaseService(TodoListDbContext context)
    {
        this.context = context;
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"TodoTask with id {taskId} not found.");

        var tag = await this.context.TodoTags.FindAsync(tagId)
            ?? throw new KeyNotFoundException($"TodoTag with id {tagId} not found.");

        if (task.Tags?.Any(t => t.Id == tag.Id) == true)
        {
            return;
        }

        task.Tags ??= [];
        task.Tags.Add(tag);

        _ = await this.context.SaveChangesAsync();
    }

    public async Task<ModelTodoTag> CreateTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be empty.");
        }

        var existingTag = await this.context.TodoTags.FirstOrDefaultAsync(t => t.Name == tagName);
        if (existingTag != null)
        {
            return new ModelTodoTag
            {
                Id = existingTag.Id,
                Name = existingTag.Name,
            };
        }

        var tagEntity = new TodoTagEntity
        {
            Name = tagName,
        };

        _ = this.context.TodoTags.Add(tagEntity);
        _ = await this.context.SaveChangesAsync();

        return new ModelTodoTag
        {
            Id = tagEntity.Id,
            Name = tagEntity.Name,
        };
    }

    public async Task DeleteTagFromTaskAsync(int taskId, int tagId)
    {
        var task = await this.context.TodoTasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new KeyNotFoundException($"TodoTask with id {taskId} not found.");

        var tag = await this.context.TodoTags.FindAsync(tagId)
            ?? throw new KeyNotFoundException($"TodoTag with id {tagId} not found in task {taskId}.");

        _ = task.Tags!.Remove(tag);
        _ = await this.context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ModelTodoTag>> GetAllTagsAsync(int page, int pageSize)
    {
        var tags = await this.context.TodoTags
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return tags.Select(t => new ModelTodoTag
        {
            Id = t.Id,
            Name = t.Name,
        });
    }

    public async Task<ModelTodoTag?> GetByIdTagAsync(int id)
    {
        var entity = await this.context.TodoTags.FindAsync(id)
            ?? throw new KeyNotFoundException($"TodoTag with id {id} not found.");

        return new ModelTodoTag
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }

    public async Task<IEnumerable<ModelTodoTask>> GetTasksByTagAsync(int tagId)
    {
        var tag = await this.context.TodoTags
            .Include(t => t.TodoTasks)
            .FirstOrDefaultAsync(t => t.Id == tagId)
            ?? throw new KeyNotFoundException($"TodoTag with id {tagId} not found.");

        return tag.TodoTasks!.Select(task => new ModelTodoTask
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            CreateDate = task.CreateDate,
            UserId = task.UserId!,
            AssignedUserId = task.AssignedUserId,
            Status = task.Status,
            TodoListId = task.TodoListId,
        });
    }
}
