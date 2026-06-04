namespace TodoListApp.WebApi.Models.Enums;

public enum NotificationType
{
    InvitationSent,
    InvitationAccepted,
    InvitationRejected,

    TaskCreated,
    TaskUpdated,
    TaskAssigned,
    TaskCompleted,

    CommentAdded,
    TagAdded,
    MemberRemoved,

    ListDeleted,
}
