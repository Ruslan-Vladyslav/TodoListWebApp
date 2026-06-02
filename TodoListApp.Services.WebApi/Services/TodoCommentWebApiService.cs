using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Services.WebApi.Services;

public class TodoCommentWebApiService : ITodoCommentService
{
    private const string BaseRoute = "TodoComment";
    private readonly HttpClient httpClient;

    public TodoCommentWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public Task<ModelTodoComment> CreateCommentAsync(int taskId, CreateTodoComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        return SendAsync<ModelTodoComment>(
            () => this.httpClient.PostAsJsonAsync(
                $"{BaseRoute}/task/{taskId}",
                comment))!;
    }

    public Task<ModelTodoComment?> GetCommentByIdAsync(int id)
    {
        return SendAsync<ModelTodoComment>(
            () => httpClient.GetAsync($"{BaseRoute}/{id}"));
    }

    public Task DeleteCommentAsync(int id)
    {
        return SendNoContentAsync(
            () => httpClient.DeleteAsync($"{BaseRoute}/{id}"));
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
