namespace TodoListApp.WebApi.Models.Helpers;

public static class NotificationMessages
{
    public static string InvitationSent(string listTitle)
        => $"You have been invited to list '{listTitle}'";

    public static string InvitationAccepted(
        string userName,
        string listTitle)
        => $"{userName} accepted invitation to '{listTitle}'";

    public static string InvitationRejected(
        string userName,
        string listTitle)
        => $"{userName} rejected invitation to '{listTitle}'";

    public static string TaskAssigned(string taskTitle)
        => $"Task '{taskTitle}' was assigned to you";

    public static string TaskCompletedForOwner(string taskTitle)
        => $"Task '{taskTitle}' was completed";

    public static string CommentAdded(string taskTitle)
        => $"New comment in task '{taskTitle}'";

    public static string ListDeleted(string listTitle)
        => $"List '{listTitle}' was deleted by owner";
}
