using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoTagController : Controller
{
    private readonly ITodoTagService _todoTagService;

    public TodoTagController(ITodoTagService todoTagService)
    {
        this._todoTagService = todoTagService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasksByTagPartial(int tagId, int page = 1)
    {
        int pageSize = 6;
        var allTasks = await this._todoTagService.GetTasksByTagAsync(tagId);

        var pagedTasks = allTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(allTasks.Count() / (double)pageSize);

        return this.PartialView("_TasksCardsPartial", pagedTasks);
    }

    public async Task<IActionResult> Index()
    {
        var tags = await this._todoTagService.GetAllTagsAsync(1, 10);

        var model = new TagsTasksViewModel
        {
            Tags = tags,
            Tasks = Enumerable.Empty<ModelTodoTask>()
        };

        return this.View(model);
    }
}
