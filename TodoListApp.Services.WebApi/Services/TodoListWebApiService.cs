using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Services.WebApi.Services
{
    public class TodoListWebApiService : ITodoListService
    {
        private const string BaseRoute = "TodoList";
        private readonly HttpClient httpClient;

        public TodoListWebApiService(HttpClient client)
        {
            this.httpClient = client;
        }

        public async Task<ModelTodoList> CreateListAsync(CreateTodoList item)
        {
            return await SendAsync<ModelTodoList>(
                () => httpClient.PostAsJsonAsync(BaseRoute, item));
        }

        public async Task<IEnumerable<ModelTodoList>> GetAllListAsync(int page, int pageSize)
        {
            var result = await SendAsync<IEnumerable<ModelTodoList>>(
                () => httpClient.GetAsync($"{BaseRoute}?page={page}&pageSize={pageSize}"));

            return result ?? Enumerable.Empty<ModelTodoList>();
        }

        public async Task<IEnumerable<ModelTodoList>> GetAllListByUserAsync(int page, int pageSize, string userId)
        {
            var result = await SendAsync<IEnumerable<ModelTodoList>>(
                () => httpClient.GetAsync($"{BaseRoute}/user/{userId}?page={page}&pageSize={pageSize}"));

            return result ?? Enumerable.Empty<ModelTodoList>();
        }

        public Task<ModelTodoList?> GetByIdListAsync(int id)
        {
            return SendAsync<ModelTodoList>(
                () => httpClient.GetAsync($"{BaseRoute}/{id}"));
        }

        public async Task UpdateListAsync(int id, UpdateTodoList item)
        {
            await SendNoContentAsync(
                () => httpClient.PutAsJsonAsync($"{BaseRoute}/{id}", item));
        }

        public async Task DeleteListAsync(int id)
        {
            await SendNoContentAsync(
                () => httpClient.DeleteAsync($"{BaseRoute}/{id}"));
        }

        private static async Task<T?> SendAsync<T>(Func<Task<HttpResponseMessage>> action)
        {
            var response = await action();

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Request failed with status {response.StatusCode}: {error}");
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
                    $"Request failed: {response.StatusCode} - {error}");
            }
        }
    }
}
