using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

public class EditPasswordViewModel
{
    [Required]
    public string? CurrentPassword { get; set; }

    [Required]
    [MinLength(6)]
    public string? NewPassword { get; set; }

    [Required]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; }
}
