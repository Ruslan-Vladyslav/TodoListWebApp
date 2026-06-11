using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTask;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoTagController : Controller
{
    private readonly ITodoTagService _todoTagService;
    private readonly IUserService _userService;

    public TodoTagController(ITodoTagService todoTagService, IUserService userService)
    {
        this._todoTagService = todoTagService;
        this._userService = userService;
    }

    public async Task<IActionResult> Index(int? tagId, int page = 1)
    {
        var pageSize = 6;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var tags = await _todoTagService.GetAllTagsAsync(1, 100);
        ViewBag.Tags = tags;

        var allTasks = tagId.HasValue
            ? await _todoTagService.GetTasksByTagAsync(tagId.Value, userId)
            : Enumerable.Empty<ModelTodoTask>();

        var totalCount = allTasks.Count();

        var pagedTasks = allTasks
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return View(new TasksPartialViewModel
        {
            Tasks = pagedTasks,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TagId = tagId
        });
    }
}
