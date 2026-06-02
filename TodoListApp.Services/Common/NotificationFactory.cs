using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Helpers;
using TodoListApp.WebApi.Models.Models.Notification;

public static class NotificationFactory
{
    public static CreateNotification TaskAssigned(
        string userId,
        string taskTitle,
        int taskId)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = NotificationMessages.TaskAssigned(taskTitle),
            Type = NotificationType.TaskAssigned,
            TodoTaskId = taskId
        };
    }

    public static CreateNotification InvitationSent(
        string userId,
        string listTitle,
        int listId)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = NotificationMessages.InvitationSent(listTitle),
            Type = NotificationType.InvitationSent,
            TodoListId = listId,
        };
    }

    public static CreateNotification InvitationAccepted(
        string userId,
        string userName,
        string listTitle,
        int listId)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = NotificationMessages.InvitationAccepted(userName, listTitle),
            Type = NotificationType.InvitationAccepted,
            TodoListId = listId,
        };
    }

    public static CreateNotification InvitationRejected(
        string userId,
        string userName,
        string listTitle,
        int listId)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = NotificationMessages.InvitationRejected(userName, listTitle),
            Type = NotificationType.InvitationRejected,
            TodoListId = listId,
        };
    }

    public static CreateNotification TaskCommented(
        string userId,
        string taskTitle,
        int taskId,
        string? senderName = null)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = senderName == null
                ? NotificationMessages.CommentAdded(taskTitle)
                : $"{senderName} commented on '{taskTitle}'",
            Type = NotificationType.CommentAdded,
            TodoTaskId = taskId,
        };
    }

    public static CreateNotification ListDeleted(
        string userId,
        string listTitle,
        int listId)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = NotificationMessages.ListDeleted(listTitle),
            Type = NotificationType.ListDeleted,
            TodoListId = listId,
        };
    }

    public static CreateNotification TaskCompleted(
        string userId,
        string taskTitle,
        int taskId)
    {
        return new CreateNotification
        {
            UserId = userId,
            Text = NotificationMessages.TaskCompletedForOwner(taskTitle),
            Type = NotificationType.TaskCompleted,
            TodoTaskId = taskId,
        };
    }
}
