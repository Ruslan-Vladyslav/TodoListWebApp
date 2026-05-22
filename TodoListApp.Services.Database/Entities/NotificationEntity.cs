using System.ComponentModel.DataAnnotations;
using TodoListApp.WebApi.Models.Enums;

public class NotificationEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    public string? UserName { get; set; }

    [Required]
    [MaxLength(500)]
    public string Text { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public NotificationType Type { get; set; }

    public int? TodoListId { get; set; }

    public int? TodoTaskId { get; set; }
}
