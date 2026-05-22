using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Notification;

public class CreateNotification
{
    public string UserId { get; set; }

    public string Text { get; set; }

    public NotificationType Type { get; set; }

    public int? TodoListId { get; set; }

    public int? TodoTaskId { get; set; }
}
