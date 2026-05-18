using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoTagController : Controller
{
    private readonly ITodoTagService _todoTagService;

    public TodoTagController(ITodoTagService todoTagService)
    {
        this._todoTagService = todoTagService;
    }

    public async Task<IActionResult> Index(int? tagId, int page = 1)
    {
        var pageSize = 6;
        var tags = await _todoTagService.GetAllTagsAsync(1, 10);

        var all = tagId.HasValue
            ? await _todoTagService.GetTasksByTagAsync(tagId.Value)
            : Enumerable.Empty<ModelTodoTask>();

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
