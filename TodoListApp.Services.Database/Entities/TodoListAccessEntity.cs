using System.ComponentModel.DataAnnotations;
using TodoListApp.Services.Database.Entity;
using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.Services.Database.Entities;

public class TodoListAccessEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TodoListId { get; set; }

    public TodoListEntity TodoList { get; set; } = null!;

    [Required]
    public string OwnerUserId { get; set; } = null!;

    public string? OwnerUserName { get; set; }

    [Required]
    public string TargetUserId { get; set; } = null!;

    public string? TargetUserName { get; set; }

    [Required]
    public TodoListRole Role { get; set; }

    public bool Accepted { get; set; }

    public DateTime SharedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? AcceptedAt { get; set; }
}
