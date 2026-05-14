using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Services.WebApi.Services;

public class TodoCommentWebApiService : ITodoCommentService
{
    private readonly HttpClient httpClient;

    public TodoCommentWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public async Task<ModelTodoComment> CreateCommentAsync(int taskId, CreateTodoComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        var response = await this.httpClient.PostAsJsonAsync($"TodoComment/task/{taskId}", comment);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API request failed with status code {response.StatusCode}: {err}");
        }

        var result = await response.Content.ReadFromJsonAsync<ModelTodoComment>();
        if (result == null)
        {
            throw new HttpRequestException("Failed to parse created comment from API response.");
        }

        return result;
    }

    public async Task<ModelTodoComment?> GetCommentByIdAsync(int id)
    {
        return await this.SafeGetAsync<ModelTodoComment>($"TodoComment/{id}");
    }

    public async Task DeleteCommentAsync(int id)
    {
        var response = await this.httpClient.DeleteAsync($"TodoComment/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API request failed with status code {response.StatusCode}: {err}");
        }
    }

    private async Task<T?> SafeGetAsync<T>(string url)
    {
        var response = await this.httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }
}
