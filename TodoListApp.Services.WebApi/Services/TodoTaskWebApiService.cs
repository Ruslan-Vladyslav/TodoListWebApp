using System.Net.Http.Json;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.WebApi.Services;

public class TodoTaskWebApiService : ITodoTaskService
{
    private const string BaseRoute = "TodoTask";
    private readonly HttpClient httpClient;

    public TodoTaskWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public async Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item)
    {
        var response = await this.httpClient.PostAsJsonAsync(BaseRoute, item);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"{response.StatusCode}: {error}");
        }

        try
        {
            return (await response.Content.ReadFromJsonAsync<ModelTodoTask>())!;
        }
        catch (System.Text.Json.JsonException)
        {
            return new ModelTodoTask();
        }
    }

    public Task DeleteTaskAsync(int id, string userId)
    {
        return SendNoContentAsync(() =>
                httpClient.DeleteAsync($"{BaseRoute}/{id}"));
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksAsync(
        int page,
        int pageSize,
        int? todoListId,
        string? userId,
        TodoTaskStatus? status,
        string? sort)
    {
        var query = $"{BaseRoute}?page={page}&pageSize={pageSize}";

        if (todoListId.HasValue)
        {
            query += $"&listId={todoListId.Value}";
        }

        if (status.HasValue)
        {
            query += $"&status={(int)status.Value}";
        }

        if (!string.IsNullOrEmpty(sort))
        {
            query += $"&sort={Uri.EscapeDataString(sort)}";
        }

        var result = await SendAsync<PagedResponse<ModelTodoTask>>(
            () => httpClient.GetAsync(query));

        return result ?? new PagedResponse<ModelTodoTask>();
    }

    public Task<ModelTodoTask?> GetByIdTaskAsync(int id, string userId)
    {
        return SendAsync<ModelTodoTask>(() =>
                httpClient.GetAsync($"{BaseRoute}/{id}"));
    }

    public Task UpdateTaskAsync(int id, UpdateTodoTask item, string userId)
    {
        return SendNoContentAsync(() =>
                httpClient.PutAsJsonAsync($"{BaseRoute}/{id}", item));
    }

    public Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId, string userId)
    {
        return SendListAsync<ModelTodoTask>(() =>
            httpClient.GetAsync($"{BaseRoute}/by-list/{todoListId}"));
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByCreateDateAsync(
    int page, int pageSize, string? userId, DateTime createDate)
    {
        var query = $"{BaseRoute}/by-create?page={page}&pageSize={pageSize}&date={createDate:O}";

        var result = await SendAsync<PagedResponse<ModelTodoTask>>(() =>
            httpClient.GetAsync(query));

        return result ?? new PagedResponse<ModelTodoTask>();
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByDueDateAsync(
    int page, int pageSize, string? userId, DateTime dueDate)
    {
        var query = $"{BaseRoute}/by-due?page={page}&pageSize={pageSize}&date={dueDate:O}";

        var result = await SendAsync<PagedResponse<ModelTodoTask>>(() =>
            httpClient.GetAsync(query));

        return result ?? new PagedResponse<ModelTodoTask>();
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByTitleAsync(
    int page, int pageSize, string? userId, string title)
    {
        var query = $"{BaseRoute}/by-title?page={page}&pageSize={pageSize}&title={Uri.EscapeDataString(title)}";

        var result = await SendAsync<PagedResponse<ModelTodoTask>>(() => httpClient.GetAsync(query));

        return result ?? new PagedResponse<ModelTodoTask>();
    }

    public async Task<PagedResponse<ModelTodoTask>> GetAllTasksByDateRangeAsync(
        int page, int pageSize, string? userId, DateTime fromDate, DateTime toDate)
    {
        var query =
            $"{BaseRoute}/by-date-range?" +
            $"page={page}&pageSize={pageSize}" +
            $"&fromDate={fromDate:O}" +
            $"&toDate={toDate:O}";

        var result = await SendAsync<PagedResponse<ModelTodoTask>>(() =>
            httpClient.GetAsync(query));

        return result ?? new PagedResponse<ModelTodoTask>();
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

    private static async Task<IEnumerable<T>> SendListAsync<T>(Func<Task<HttpResponseMessage>> action)
    {
        var result = await SendAsync<IEnumerable<T>>(action);
        return result ?? Enumerable.Empty<T>();
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
