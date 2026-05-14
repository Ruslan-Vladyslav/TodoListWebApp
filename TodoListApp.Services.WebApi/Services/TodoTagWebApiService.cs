using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Services.WebApi.Services;

public class TodoTagWebApiService : ITodoTagService
{
    private readonly HttpClient httpClient;

    public TodoTagWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public async Task<IEnumerable<ModelTodoTag>> GetAllTagsAsync(int page, int pageSize)
    {
        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTag>>($"TodoTag?page={page}&pageSize={pageSize}");
        return result ?? Enumerable.Empty<ModelTodoTag>();
    }

    public Task<ModelTodoTag?> GetByIdTagAsync(int id)
    {
        return this.SafeGetAsync<ModelTodoTag>($"TodoTag/{id}");
    }

    public async Task<IEnumerable<ModelTodoTask>> GetTasksByTagAsync(int tagId)
    {
        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>($"TodoTag/{tagId}/tasks");
        return result ?? Enumerable.Empty<ModelTodoTask>();
    }

    public async Task AddTagToTaskAsync(int taskId, int tagId)
    {
        var url = $"TodoTag/{tagId}/addTask/{taskId}";
        var response = await this.httpClient.PostAsync(url, null);

        _ = response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTagFromTaskAsync(int taskId, int tagId)
    {
        _ = await this.SafeDeleteAsync($"TodoTag/{tagId}/removeTask/{taskId}");
    }

    public async Task<ModelTodoTag> CreateTagAsync(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            throw new ArgumentException("Tag name cannot be empty.");
        }

        var newTag = new { Name = tagName };

        var response = await this.httpClient.PostAsJsonAsync("TodoTag", newTag);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to create tag. Status: {response.StatusCode}. Message: {error}");
        }

        var createdTag = await response.Content.ReadFromJsonAsync<ModelTodoTag>();

        if (createdTag == null)
        {
            throw new HttpRequestException("Failed to read created tag from API response.");
        }

        return createdTag;
    }

    private async Task<T?> SafeGetAsync<T>(string url)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, url);
        var response = await this.httpClient.GetAsync(uri);

        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<bool> SafeDeleteAsync(string url)
    {
        var uri = new Uri(this.httpClient.BaseAddress!, url);
        var response = await this.httpClient.DeleteAsync(uri);

        return response.IsSuccessStatusCode;
    }
}
