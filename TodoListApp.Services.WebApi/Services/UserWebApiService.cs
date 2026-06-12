using System.Net;
using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.User;

namespace TodoListApp.Services.WebApi.Services;

public class UserWebApiService : IUserService
{
    private const string BaseRoute = "User";
    private readonly HttpClient _http;

    public UserWebApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<UserModel>> GetAllAsync()
    {
        return await SendAsync<List<UserModel>>(
            () => _http.GetAsync(BaseRoute))
            ?? new List<UserModel>();
    }

    public async Task<UserModel?> GetByIdAsync(string id)
    {
        return await SendAsync<UserModel>(
            () => _http.GetAsync($"{BaseRoute}/{id}"));
    }

    private static async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await SendAsync<UserModel>(
            () => _http.GetAsync($"{BaseRoute}/{userId}"));

        return user?.UserName;
    }

    public async Task<UserModel?> GetByEmailAsync(string email)
    {
        var response = await _http.GetAsync(
            $"{BaseRoute}/by-email/{Uri.EscapeDataString(email)}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        return await response.Content.ReadFromJsonAsync<UserModel>();
    }

    public async Task<UserModel?> GetByUserNameAsync(string userName)
    {
        var response = await _http.GetAsync(
            $"{BaseRoute}/by-username/{Uri.EscapeDataString(userName)}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        return await response.Content.ReadFromJsonAsync<UserModel>();
    }

    public async Task<Dictionary<string, string?>> GetUsersByIdsAsync(List<string> ids)
    {
        return await SendAsync<Dictionary<string, string?>>(
            () => _http.PostAsJsonAsync($"{BaseRoute}/by-ids", ids))
            ?? new();
    }

    public async Task DeleteAsync(string userId)
    {
        await SendAsync<object>(
            () => _http.DeleteAsync($"{BaseRoute}/{userId}"));
    }

    public async Task UpdateUserNameAsync(
        string userId,
        string userName)
    {
        var response = await _http.PutAsJsonAsync(
            $"{BaseRoute}/{userId}/username",
            userName);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task<bool> UserNameExistsAsync(
    string userName)
    {
        return await SendAsync<bool>(
            () => _http.GetAsync(
                $"{BaseRoute}/exists/{userName}"));
    }
}
