using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Notification;

namespace TodoListApp.Services.Database.Services;

public class NotificationDatabaseService : INotificationService
{
    private readonly TodoListDbContext _context;

    public NotificationDatabaseService(TodoListDbContext context)
    {
        this._context = context;
    }

    public async Task<ModelNotification> CreateAsync(CreateNotification item)
    {
        var entity = new NotificationEntity
        {
            UserId = item.UserId,
            SenderUserId = item.SenderUserId,
            SenderUserName = item.SenderUserName,
            Text = item.Text,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            Type = item.Type,
            TodoListId = item.TodoListId,
            TodoTaskId = item.TodoTaskId,
        };

        await _context.Notifications.AddAsync(entity);
        await _context.SaveChangesAsync();

        return new ModelNotification
        {
            Id = entity.Id,
            UserId = entity.UserId,
            SenderUserId = entity.SenderUserId,
            SenderUserName = entity.SenderUserName,
            Text = entity.Text,
            IsRead = entity.IsRead,
            CreateDate = entity.CreatedAt,
            Type = entity.Type,
            TodoListId = entity.TodoListId,
            TodoTaskId = entity.TodoTaskId,
        };
    }

    public async Task<IEnumerable<ModelNotification>> GetUserNotificationsAsync(string userId)
    {
        var items = await this._context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return items.Select(x => new ModelNotification
        {
            Id = x.Id,
            UserId = x.UserId,
            SenderUserId = x.SenderUserId,
            SenderUserName = x.SenderUserName,
            Text = x.Text,
            IsRead = x.IsRead,
            CreateDate = x.CreatedAt,
            Type = x.Type,
            TodoListId = x.TodoListId,
            TodoTaskId = x.TodoTaskId,
        });
    }

    public async Task MarkAsReadAsync(int id)
    {
        var entity = await this._context.Notifications.FindAsync(id);

        if (entity == null)
        {
            throw new KeyNotFoundException("Notification not found");
        }

        entity.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Notifications.FindAsync(id);

        if (entity == null)
        {
            throw new KeyNotFoundException("Notification not found");
        }

        _context.Notifications.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
