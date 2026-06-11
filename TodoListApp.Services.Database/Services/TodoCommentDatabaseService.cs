using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Services.Database.Services;

public class TodoCommentDatabaseService : ITodoCommentService
{
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly TodoListDbContext _context;

    public TodoCommentDatabaseService(TodoListDbContext context, INotificationService notificationService, IUserService userService)
    {
        this._context = context;
        this._notificationService = notificationService;
        this._userService = userService;
    }

    public async Task<ModelTodoComment> CreateCommentAsync(int taskId, CreateTodoComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var task = await this._context.TodoTasks
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
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

        await _context.TodoComments.AddAsync(entity);
        await _context.SaveChangesAsync();

        var list = await this._context.TodoLists
            .AsNoTracking()
            .FirstAsync(l => l.Id == task.TodoListId);

        var recipients = new HashSet<string>();

        if (list.UserId != comment.UserId)
        {
            _ = recipients.Add(list.UserId);
        }

        if (!string.IsNullOrEmpty(task.AssignedToUserId) &&
            task.AssignedToUserId != comment.UserId)
        {
            _ = recipients.Add(task.AssignedToUserId);
        }

        var senderName = await this._userService.GetUserNameAsync(comment.UserId) ?? "Unknown";

        foreach (var userId in recipients)
        {
            _ = await this._notificationService.CreateAsync(
                NotificationFactory.TaskCommented(
                    userId,
                    task.Title,
                    task.Id,
                    senderName: senderName));
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

    public async Task<ModelTodoComment?> GetCommentByIdAsync(int id)
    {
        var entity = await this._context.TodoComments
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
        var entity = await this._context.TodoComments.FindAsync(id)
            ?? throw new KeyNotFoundException($"TodoComment with id {id} not found.");

        _ = this._context.TodoComments.Remove(entity);
        _ = await this._context.SaveChangesAsync();
    }
}
