using System.ComponentModel.DataAnnotations;

namespace TodoListApp.WebApp.Models.Account;

public class EditProfileViewModel
{
    [Required]
    public string Username { get; set; }

    public string Email { get; set; }
}
