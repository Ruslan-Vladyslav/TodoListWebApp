using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Services.WebApi.Services
{
    public class TodoListWebApiService : ITodoListService
    {
        private readonly HttpClient httpClient;

        public TodoListWebApiService(HttpClient client)
        {
            this.httpClient = client;
        }

        public Task<ModelTodoList> CreateListAsync(CreateTodoList item)
        {
            return this.SafePostAsync<ModelTodoList>("TodoList", item)!;
        }

        public async Task<IEnumerable<ModelTodoList>> GetAllListAsync(int page, int pageSize)
        {
            var result = await this.SafeGetAsync<IEnumerable<ModelTodoList>>($"TodoList?page={page}&pageSize={pageSize}");
            return result ?? Enumerable.Empty<ModelTodoList>();
        }

        public async Task<IEnumerable<ModelTodoList>> GetAllListByUserAsync(int page, int pageSize, string userId)
        {
            var result = await this.SafeGetAsync<IEnumerable<ModelTodoList>>($"TodoList/user/{userId}?page={page}&pageSize={pageSize}");
            return result ?? Enumerable.Empty<ModelTodoList>();
        }

        public Task<ModelTodoList?> GetByIdListAsync(int id)
        {
            return this.SafeGetAsync<ModelTodoList>($"TodoList/{id}");
        }

        public Task UpdateListAsync(int id, UpdateTodoList item)
        {
            return this.SafePutAsync($"TodoList/{id}", item);
        }

        public Task DeleteListAsync(int id)
        {
            return this.SafeDeleteAsync($"TodoList/{id}");
        }

        private async Task<bool> SafeDeleteAsync(string url)
        {
            var uri = new Uri(this.httpClient.BaseAddress!, url);
            var response = await this.httpClient.DeleteAsync(uri);

            return response.IsSuccessStatusCode;
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

        private async Task<T?> SafePostAsync<T>(string url, object body)
        {
            var response = await this.httpClient.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>();
        }

        private async Task<bool> SafePutAsync(string url, object body)
        {
            var response = await this.httpClient.PutAsJsonAsync(url, body);
            return response.IsSuccessStatusCode;
        }
    }
}
