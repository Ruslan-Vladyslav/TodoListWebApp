using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.WebApi.Services;

public class TodoTagWebApiService : ITodoTagService
{
    private const string BaseRoute = "TodoTag";
    private readonly HttpClient httpClient;

    public TodoTagWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public async Task<PagedResponse<ModelTodoTag>> GetAllTagsAsync(int page, int pageSize)
    {
        var result = await SendAsync<PagedResponse<ModelTodoTag>>(
           () => httpClient.GetAsync($"{BaseRoute}?page={page}&pageSize={pageSize}"));

        return result ?? new PagedResponse<ModelTodoTag>();
    }

    public Task<ModelTodoTag?> GetByIdTagAsync(int id)
    {
        return SendAsync<ModelTodoTag>(
            () => httpClient.GetAsync($"{BaseRoute}/{id}"));
    }

    public Task<ModelTodoTag> CreateTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be empty.");
        }

        return SendAsync<ModelTodoTag>(
            () => httpClient.PostAsJsonAsync(
                BaseRoute,
                new { Name = tagName }))!;
    }

    public async Task<IEnumerable<ModelTodoTask>> GetTasksByTagAsync(int tagId, string userId)
    {
        return await SendAsync<IEnumerable<ModelTodoTask>>(
            () => httpClient.GetAsync($"{BaseRoute}/{tagId}/tasks"))
            ?? Enumerable.Empty<ModelTodoTask>();
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId, string userId)
    {
        await SendNoContentAsync(() =>
            httpClient.PostAsync($"{BaseRoute}/tasks/{taskId}/tags/{tagId}", null));
    }

    public async Task DeleteTagFromTaskAsync(int taskId, int tagId, string userId)
    {
        await SendNoContentAsync(() =>
            httpClient.DeleteAsync($"{BaseRoute}/tasks/{taskId}/tags/{tagId}"));
    }

    private static async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"{response.StatusCode}: {error}");
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    private static async Task SendNoContentAsync(Func<Task<HttpResponseMessage>> action)
    {
        var response = await action();

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"{response.StatusCode}: {error}");
        }
    }
}
