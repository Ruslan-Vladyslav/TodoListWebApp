using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTask;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoTagController : Controller
{
    private readonly ITodoTagService _todoTagService;
    private readonly UserManager<IdentityUser> _userManager;

    public TodoTagController(ITodoTagService todoTagService, UserManager<IdentityUser> userManager)
    {
        this._todoTagService = todoTagService;
        this._userManager = userManager;
    }

    public async Task<IActionResult> Index(int? tagId, int page = 1)
    {
        var pageSize = 6;
        var userId = _userManager.GetUserId(User);
        var tags = await _todoTagService.GetAllTagsAsync(page, pageSize);

        var all = tagId.HasValue
            ? await _todoTagService.GetTasksByTagAsync(tagId.Value)
            : Enumerable.Empty<ModelTodoTask>();
        all = all.Where(t => t.UserId == userId);

        var totalPages = (int)Math.Ceiling(all.Count() / (double)pageSize);

        var tasks = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.Tags = tags;

        return this.View(new TasksPartialViewModel
        {
            Tasks = tasks,
            CurrentPage = page,
            TotalPages = totalPages,
            TagId = tagId,
            Action = "Index",
            Controller = "TodoTag",
        });
    }
}
