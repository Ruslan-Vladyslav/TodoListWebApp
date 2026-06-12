using Microsoft.AspNetCore.Authentication.Cookies;
using TodoListApp.Services.Interfaces;
using TodoListApp.Services.WebApi.Services;
using TodoListApp.WebApp.Services.Email;

var builder = WebApplication.CreateBuilder(args);

var apiUri = builder.Configuration["WebApi:ApiUri"]
    ?? throw new InvalidOperationException("WebApi:ApiUri missing");

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthDelegatingHandler>();

builder.Services.AddHttpClient<ITodoListService, TodoListWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();


builder.Services.AddHttpClient<ITodoTaskService, TodoTaskWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<ITodoTagService, TodoTagWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<ITodoCommentService, TodoCommentWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<IAccessService, AccessWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<IInvitationService, InvitationWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<INotificationService, NotificationWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
}).AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<IUserService, UserWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
})
.AddHttpMessageHandler<AuthDelegatingHandler>();

builder.Services.AddHttpClient<IAuthService, AuthWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
})
.AddHttpMessageHandler<AuthDelegatingHandler>();


builder.Services.AddScoped<EmailHandler>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Home/Error");
    _ = app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
