using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
