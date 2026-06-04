namespace TodoListApp.WebApi.Models.Models.Auth
{
    public class AuthResponse
    {
        public bool IsSuccessful { get; set; }

        public string? Token { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
