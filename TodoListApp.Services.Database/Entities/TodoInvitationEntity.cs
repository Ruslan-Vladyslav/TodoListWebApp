using System.ComponentModel.DataAnnotations;
using TodoListApp.Services.Database.Entity;
using TodoListApp.WebApi.Models.Enums;

public class TodoInvitationEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TodoListId { get; set; }

    public TodoListEntity TodoList { get; set; } = null!;

    [Required]
    public string SenderUserId { get; set; } = null!;

    public string? SenderUserName { get; set; }

    [Required]
    public string ReceiverUserId { get; set; } = null!;

    public string? ReceiverUserName { get; set; }

    [Required]
    public TodoListRole Role { get; set; }

    [Required]
    public InvitationStatus Status { get; set; }
        = InvitationStatus.Pending;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }

    public string? Message { get; set; }
}
