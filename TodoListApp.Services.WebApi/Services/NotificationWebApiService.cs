using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Notification;

namespace TodoListApp.Services.WebApi.Services;

public class NotificationWebApiService : INotificationService
{
    private const string BaseRoute = "notifications";
    private readonly HttpClient httpClient;

    public NotificationWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public async Task<ModelNotification> CreateAsync(CreateNotification item)
    {
        var response = await this.httpClient.PostAsJsonAsync(BaseRoute, item);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }

        return (await response.Content.ReadFromJsonAsync<ModelNotification>())!;
    }

    public async Task<IEnumerable<ModelNotification>> GetUserNotificationsAsync(string userId)
    {
        var result = await this.httpClient.GetFromJsonAsync<IEnumerable<ModelNotification>>(
            $"{BaseRoute}/user");

        return result ?? Enumerable.Empty<ModelNotification>();
    }

    public async Task MarkAsReadAsync(int id)
    {
        var response = await httpClient.PostAsync($"{BaseRoute}/{id}/read", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }
    }

    public async Task DeleteAsync(int id)
    {
        var response = await httpClient.DeleteAsync($"{BaseRoute}/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync());
        }
    }
}
