using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Access;

namespace TodoListApp.Services.WebApi.Services;

public class AccessWebApiService : IAccessService
{
    private const string BaseRoute = "Access";
    private readonly HttpClient httpClient;

    public AccessWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public Task GrantAccessAsync(string ownerUserId, string userId, int listId, TodoListRole role)
    {
        return SendNoContentAsync(() =>
            httpClient.PostAsync(
                $"{BaseRoute}/grant?userId={userId}&listId={listId}&role={(int)role}",
                null));
    }

    public Task RevokeAccessAsync(string ownerUserId, string targetUserId, int listId)
    {
        return SendNoContentAsync(() =>
            httpClient.DeleteAsync(
                $"{BaseRoute}/revoke?targetUserId={targetUserId}&listId={listId}"));
    }

    public async Task<IEnumerable<ModelTodoListAccess>> GetAccessListAsync(int listId)
    {
        return await SendAsync<
            IEnumerable<ModelTodoListAccess>>(
            () => httpClient.GetAsync(
                $"{BaseRoute}/members?listId={listId}"))
            ?? Enumerable.Empty<ModelTodoListAccess>();
    }

    public async Task<TodoListRole> GetUserRoleAsync(string userId, int listId)
    {
        var response = await httpClient.GetAsync(
            $"{BaseRoute}/role?listId={listId}");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {error}");
        }

        var role = await response.Content.ReadFromJsonAsync<TodoListRole?>();

        return role ?? TodoListRole.Viewer;
    }

    private static async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {error}");
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    private static async Task SendNoContentAsync(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {error}");
        }
    }
}
