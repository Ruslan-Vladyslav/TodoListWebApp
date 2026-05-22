using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Services.Database.Services;

public class TodoCommentDatabaseService : ITodoCommentService
{
    private readonly TodoListDbContext context;

    public TodoCommentDatabaseService(TodoListDbContext context)
    {
        this.context = context;
    }

    public async Task<ModelTodoComment> CreateCommentAsync(int taskId, CreateTodoComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var taskExists = await context.TodoTasks
            .AnyAsync(t => t.Id == taskId);

        if (!taskExists)
        {
            throw new KeyNotFoundException($"TodoTask {taskId} not found");
        }

        var entity = new TodoCommentEntity
        {
            Text = comment.Text,
            CreateDate = DateTime.UtcNow,
            UserId = comment.UserId,
            TodoTaskId = taskId,
        };

        var created = await this.context.TodoComments.AddAsync(entity);
        _ = await this.context.SaveChangesAsync();

        return new ModelTodoComment
        {
            Id = created.Entity.Id,
            Text = created.Entity.Text,
            CreateDate = created.Entity.CreateDate,
            UserId = created.Entity.UserId,
            TodoTaskId = created.Entity.TodoTaskId,
        };
    }

    public async Task<ModelTodoComment?> GetCommentByIdAsync(int id)
    {
        var entity = await this.context.TodoComments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (entity == null)
        {
            throw new KeyNotFoundException($"TodoComment with id {id} not found.");
        }

        return new ModelTodoComment
        {
            Id = entity.Id,
            Text = entity.Text,
            CreateDate = entity.CreateDate,
            UserId = entity.UserId,
            TodoTaskId = entity.TodoTaskId,
        };
    }

    public async Task DeleteCommentAsync(int id)
    {
        var entity = await this.context.TodoComments.FindAsync(id)
            ?? throw new KeyNotFoundException($"TodoComment with id {id} not found.");

        _ = this.context.TodoComments.Remove(entity);
        _ = await this.context.SaveChangesAsync();
    }
}
