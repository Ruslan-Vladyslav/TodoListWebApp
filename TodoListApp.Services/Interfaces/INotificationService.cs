using TodoListApp.WebApi.Models.Models.Notification;

namespace TodoListApp.Services.Interfaces;

public interface INotificationService
{
    Task<ModelNotification> CreateAsync(CreateNotification item);

    Task<IEnumerable<ModelNotification>> GetUserNotificationsAsync(string userId);

    Task MarkAsReadAsync(int id);

    Task DeleteAsync(int id);
}
