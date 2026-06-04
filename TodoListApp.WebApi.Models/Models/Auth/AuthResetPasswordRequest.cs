namespace TodoListApp.WebApi.Models.Models.Auth;

public class AuthResetPasswordRequest
{
    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string NewPassword { get; set; } = null!;
}
