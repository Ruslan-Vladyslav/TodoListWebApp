using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Auth;

public class AuthWebApiService : IAuthService
{
    private readonly HttpClient _http;

    public AuthWebApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> RegisterAsync(UserRegisterRequest request)
        => await _http.PostAsJsonAsync("Auth/register", request)
            .ContinueWith(r => r.Result.Content.ReadFromJsonAsync<AuthResponse>())
            .Unwrap();

    public async Task<AuthResponse> LoginAsync(UserLoginRequest request)
        => await _http.PostAsJsonAsync("Auth/login", request)
            .ContinueWith(r => r.Result.Content.ReadFromJsonAsync<AuthResponse>())
            .Unwrap();

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var res = await _http.PostAsJsonAsync("Auth/change-password",
            new { userId, currentPassword, newPassword });

        return res.IsSuccessStatusCode;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var res = await _http.PostAsJsonAsync("Auth/reset-token", email);
        return await res.Content.ReadFromJsonAsync<string>();
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var res = await _http.PostAsJsonAsync("Auth/reset-password",
            new { email, token, newPassword });

        return res.IsSuccessStatusCode;
    }
}
