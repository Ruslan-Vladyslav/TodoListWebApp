using System.Net.Http.Json;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.Services.WebApi.Services;

public class InvitationWebApiService : IInvitationService
{
    private const string BaseRoute = "Invitation";
    private readonly HttpClient httpClient;

    public InvitationWebApiService(HttpClient client)
    {
        this.httpClient = client;
    }

    public async Task SendInvitationAsync(
        string senderId,
        string receiverId,
        int listId,
        TodoListRole role,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(senderId))
        {
            throw new ArgumentException("SenderId is required");
        }

        if (string.IsNullOrWhiteSpace(receiverId))
        {
            throw new ArgumentException("ReceiverId is required");
        }

        var request = new SendInvitationRequest
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            ListId = listId,
            Role = role,
            Message = message,
        };

        await SendNoContentAsync(() =>
            this.httpClient.PostAsJsonAsync($"{BaseRoute}/send", request));
    }

    public async Task<bool> HasPendingInvitationAsync(int listId, string receiverId)
    {
        var response = await this.httpClient.GetAsync(
            $"{BaseRoute}/pending?listId={listId}&receiverId={Uri.EscapeDataString(receiverId)}");

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        return await response.Content.ReadFromJsonAsync<bool>();
    }

    public async Task AcceptInvitationAsync(int invitationId)
    {
        await SendNoContentAsync(() =>
            httpClient.PostAsync($"{BaseRoute}/accept/{invitationId}", null));
    }

    public async Task RejectInvitationAsync(int invitationId)
    {
        await SendNoContentAsync(() =>
            httpClient.PostAsync($"{BaseRoute}/reject/{invitationId}", null));
    }

    public async Task<IEnumerable<ModelInvitation>> GetUserInvitationsAsync(string userId)
    {
        return await SendAsync<IEnumerable<ModelInvitation>>(
            () => httpClient.GetAsync($"{BaseRoute}/user"))
            ?? Enumerable.Empty<ModelInvitation>();
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
