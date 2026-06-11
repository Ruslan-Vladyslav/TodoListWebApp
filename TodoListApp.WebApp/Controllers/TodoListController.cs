using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoListController : Controller
{
    private readonly ITodoListService _todoListService;
    private readonly IAccessService _accessService;

    private readonly int pageSize = 8;

    public TodoListController(ITodoListService todoListService, IAccessService accessService)
    {
        this._todoListService = todoListService;
        this._accessService = accessService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        ViewBag.CurrentUserId = currentUserId;

        var allLists =
            await _todoListService.GetAllListByUserAsync(
                page,
                pageSize,
                currentUserId);

        var paged = allLists.Items.ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = allLists.TotalPages;

        return View(paged);
    }

    public async Task<IActionResult> Details(int id)
    {
        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var role = await _accessService.GetUserRoleAsync(
            currentUserId!,
            list.Id);

        ViewBag.Role = role;

        return View(list);
    }

    public IActionResult Create()
    {
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoList model)
    {
        model.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _ = await this._todoListService.CreateListAsync(model);
        return this.RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (list.UserId != userId)
        {
            return Forbid();
        }

        var updateModel = new UpdateTodoList
        {
            Id = list.Id,
            Title = list.Title,
            Description = list.Description
        };

        return View(updateModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTodoList model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        if (list.UserId != userId)
        {
            return Forbid();
        }

        await _todoListService.UpdateListAsync(id, model);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DeleteConfirm(int id)
    {
        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (list.UserId != userId)
        {
            return Forbid();
        }

        return View(list);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var list = await _todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (list.UserId != userId)
        {
            return Forbid();
        }

        await _todoListService.DeleteListAsync(id);

        return RedirectToAction(nameof(Index));
    }
}
