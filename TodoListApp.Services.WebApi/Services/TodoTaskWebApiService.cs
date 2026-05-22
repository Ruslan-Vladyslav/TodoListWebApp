//using System.Net.Http.Json;
//using TodoListApp.Services.Enums;
//using TodoListApp.Services.Interfaces;
//using TodoListApp.WebApi.Models.Models.TodoTask;

//namespace TodoListApp.Services.WebApi.Services;

//public class TodoTaskWebApiService : ITodoTaskService
//{
//    private readonly HttpClient httpClient;

//    public TodoTaskWebApiService(HttpClient client)
//    {
//        this.httpClient = client;
//    }

//    public Task<ModelTodoTask> CreateTaskAsync(CreateTodoTask item)
//    {
//        return this.SafePostAsync<ModelTodoTask>("TodoTask", item)!;
//    }

//    public Task DeleteTaskAsync(int id)
//    {
//        return this.SafeDeleteAsync($"TodoTask/{id}");
//    }

//    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksAsync(int page, int pageSize, int? toDoListId, string? userId, TodoTaskStatus? status, string? sort)
//    {
//        var query = $"TodoTask?page={page}&pageSize={pageSize}";

//        if (toDoListId.HasValue)
//        {
//            query += $"&listId={toDoListId}";
//        }

//        if (!string.IsNullOrEmpty(userId))
//        {
//            query += $"&userId={userId}";
//        }

//        if (status.HasValue)
//        {
//            query += $"&status={(int)status.Value}";
//        }

//        if (!string.IsNullOrEmpty(sort))
//        {
//            query += $"&sort={Uri.EscapeDataString(sort)}";
//        }

//        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>(query);
//        return result ?? Enumerable.Empty<ModelTodoTask>();
//    }

//    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByCreateDateAsync(int page, int pageSize, string? userId, DateTime createDate)
//    {
//        var query = $"TodoTask/by-create?page={page}&pageSize={pageSize}&date={createDate:O}";

//        if (!string.IsNullOrEmpty(userId))
//        {
//            query += $"&userId={userId}";
//        }

//        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>(query);
//        return result ?? Enumerable.Empty<ModelTodoTask>();
//    }

//    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByDueDateAsync(int page, int pageSize, string? userId, DateTime dueDate)
//    {
//        var query = $"TodoTask/by-due?page={page}&pageSize={pageSize}&date={dueDate:O}";

//        if (!string.IsNullOrEmpty(userId))
//        {
//            query += $"&userId={userId}";
//        }

//        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>(query);
//        return result ?? Enumerable.Empty<ModelTodoTask>();
//    }

//    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByTitleAsync(int page, int pageSize, string? userId, string title)
//    {
//        var query = $"TodoTask/by-title?page={page}&pageSize={pageSize}&title={Uri.EscapeDataString(title)}";

//        if (!string.IsNullOrEmpty(userId))
//        {
//            query += $"&userId={userId}";
//        }

//        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>(query);
//        return result ?? Enumerable.Empty<ModelTodoTask>();
//    }

//    public Task<ModelTodoTask?> GetByIdTaskAsync(int id)
//    {
//        return this.SafeGetAsync<ModelTodoTask>($"TodoTask/{id}");
//    }

//    public Task UpdateTaskAsync(int id, UpdateTodoTask item)
//    {
//        return this.SafePutAsync($"TodoTask/{id}", item);
//    }

//    public async Task<IEnumerable<ModelTodoTask>> GetByListIdAsync(int todoListId)
//    {
//        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>($"TodoTask/by-list/{todoListId}");
//        return result ?? Enumerable.Empty<ModelTodoTask>();
//    }

//    private async Task<T?> SafeGetAsync<T>(string url)
//    {
//        var uri = new Uri(this.httpClient.BaseAddress!, url);
//        var response = await this.httpClient.GetAsync(uri);

//        if (!response.IsSuccessStatusCode)
//        {
//            return default;
//        }

//        return await response.Content.ReadFromJsonAsync<T>();
//    }

//    private async Task<T?> SafePostAsync<T>(string url, object body)
//    {
//        var response = await this.httpClient.PostAsJsonAsync(url, body);

//        if (!response.IsSuccessStatusCode)
//        {
//            return default;
//        }

//        return await response.Content.ReadFromJsonAsync<T>();
//    }

//    private async Task<bool> SafePutAsync(string url, object body)
//    {
//        var response = await this.httpClient.PutAsJsonAsync(url, body);
//        return response.IsSuccessStatusCode;
//    }

//    private async Task<bool> SafeDeleteAsync(string url)
//    {
//        var uri = new Uri(this.httpClient.BaseAddress!, url);
//        var response = await this.httpClient.DeleteAsync(uri);
//        return response.IsSuccessStatusCode;
//    }

//    public async Task<IEnumerable<ModelTodoTask>> GetAllTasksByDateRangeAsync(int page, int pageSize, string? userId, DateTime fromDate, DateTime toDate)
//    {
//        var query =
//            $"TodoTask/by-date-range?" +
//            $"page={page}" +
//            $"&pageSize={pageSize}" +
//            $"&fromDate={fromDate:O}" +
//            $"&toDate={toDate:O}";

//        if (!string.IsNullOrEmpty(userId))
//        {
//            query += $"&userId={Uri.EscapeDataString(userId)}";
//        }

//        var result = await this.SafeGetAsync<IEnumerable<ModelTodoTask>>(query);
//        return result ?? Enumerable.Empty<ModelTodoTask>();
//    }
//}
