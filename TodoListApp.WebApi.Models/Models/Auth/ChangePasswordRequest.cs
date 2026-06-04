namespace TodoListApp.WebApi.Models.Models.Auth;

public class ChangePasswordRequest
{
    public string UserId { get; set; } = null!;

    public string CurrentPassword { get; set; } = null!;

    public string NewPassword { get; set; } = null!;
}
