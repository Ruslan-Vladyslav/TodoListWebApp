using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

