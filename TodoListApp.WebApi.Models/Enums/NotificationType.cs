using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApi.Models.Enums;

public enum NotificationType
{
    [Display(Name = "Invitation Sent")]
    InvitationSent,

    [Display(Name = "Invitation Accepted")]
    InvitationAccepted,

    [Display(Name = "Invitation Rejected")]
    InvitationRejected,

    [Display(Name = "Task Created")]
    TaskCreated,

    [Display(Name = "Task Updated")]
    TaskUpdated,

    [Display(Name = "Task Assigned")]
    TaskAssigned,

    [Display(Name = "Task Completed")]
    TaskCompleted,

    [Display(Name = "Comment Added")]
    CommentAdded,

    [Display(Name = "Tag Added")]
    TagAdded,

    [Display(Name = "Member Removed")]
    MemberRemoved,

    [Display(Name = "List Deleted")]
    ListDeleted,
}
