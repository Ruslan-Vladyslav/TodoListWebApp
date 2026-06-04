using TodoListApp.WebApi.Models.Models.Auth;

namespace TodoListApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(UserRegisterRequest request);

        Task<AuthResponse> LoginAsync(UserLoginRequest request);

        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        Task<string?> GeneratePasswordResetTokenAsync(string email);

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
