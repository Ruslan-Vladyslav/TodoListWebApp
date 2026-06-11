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
    {
        var response = await _http.PostAsJsonAsync("Auth/register", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new AuthResponse { IsSuccessful = false, ErrorMessage = error };
        }
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public async Task<AuthResponse> LoginAsync(UserLoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("Auth/login", request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new AuthResponse { IsSuccessful = false, ErrorMessage = error };
        }
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var res = await _http.PostAsJsonAsync("Auth/change-password",
            new { userId, currentPassword, newPassword });

        return res.IsSuccessStatusCode;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var res = await _http.PostAsJsonAsync("Auth/reset-token", new
        {
            email
        });

        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            Console.WriteLine(error);
            return null;
        }

        return await res.Content.ReadAsStringAsync();
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var res = await _http.PostAsJsonAsync("Auth/reset-password",
            new { email, token, newPassword });

        return res.IsSuccessStatusCode;
    }
}
