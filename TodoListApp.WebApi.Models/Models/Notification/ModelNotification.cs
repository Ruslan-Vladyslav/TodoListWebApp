using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApi.Models.Models.Notification;

public class ModelNotification
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public string Text { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreateDate { get; set; }

    public NotificationType Type { get; set; }

    public int? TodoListId { get; set; }

    public int? TodoTaskId { get; set; }
}
