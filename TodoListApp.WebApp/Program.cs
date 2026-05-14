using Microsoft.AspNetCore.Identity;
using TodoListApp.Services.Interfaces;
using TodoListApp.Services.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.DbContexts;

var builder = WebApplication.CreateBuilder(args);

var apiUri = builder.Configuration["WebApi:ApiUri"]!;

builder.Services.AddHttpClient<ITodoListService, TodoListWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
});


builder.Services.AddHttpClient<ITodoTaskService, TodoTaskWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
});

builder.Services.AddHttpClient<ITodoTagService, TodoTagWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
});

builder.Services.AddHttpClient<ITodoCommentService, TodoCommentWebApiService>(client =>
{
    client.BaseAddress = new Uri(apiUri);
});

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();


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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
