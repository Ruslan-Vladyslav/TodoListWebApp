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

    public async Task GrantAccessAsync(string ownerUserId, string userId, int listId, TodoListRole role)
    {
        await SendNoContentAsync(() =>
            httpClient.PostAsync(
                $"{BaseRoute}/grant?ownerUserId={ownerUserId}&userId={userId}&listId={listId}&role={role}", null));
    }

    public async Task RevokeAccessAsync(string ownerUserId, string targetUserId, int listId)
    {
        await SendNoContentAsync(() =>
            httpClient.DeleteAsync(
                $"{BaseRoute}/revoke?ownerUserId={ownerUserId}&targetUserId={targetUserId}&listId={listId}"));
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
            $"{BaseRoute}/role?userId={userId}&listId={listId}");

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
