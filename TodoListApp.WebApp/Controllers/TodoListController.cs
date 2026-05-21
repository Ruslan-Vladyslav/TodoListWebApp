using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoListController : Controller
{
    private readonly ITodoListService _todoListService;
    private readonly UserManager<IdentityUser> _userManager;

    private readonly int pageSize = 8;

    public TodoListController(ITodoListService todoListService, UserManager<IdentityUser> userManager)
    {
        this._todoListService = todoListService;
        this._userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var allLists = await _todoListService.GetAllListByUserAsync(1, int.MaxValue, userId);

        var user = await _userManager.GetUserAsync(User);
        var userName = user?.UserName;

        foreach (var list in allLists)
        {
            list.UserName = userName;
        }

        var paged = allLists
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(allLists.Count() / (double)pageSize);

        return View(paged);
    }

    public async Task<IActionResult> Details(int id)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View("Error");
        }
        var list = await this._todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        list.UserName = user?.UserName;

        return this.View(list);
    }

    public IActionResult Create()
    {
        return this.View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoList model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        model.UserId = _userManager.GetUserId(User);

        _ = await this._todoListService.CreateListAsync(model);
        return this.RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View("Error");
        }

        var list = await this._todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return this.NotFound();
        }

        var updateModel = new UpdateTodoList
        {
            Title = list.Title!,
            Description = list.Description!,
        };

        return this.View(updateModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTodoList model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
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
        if (!this.ModelState.IsValid)
        {
            return this.View("Error");
        }

        var list = await this._todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return this.NotFound();
        }

        return this.View(list);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View("Error");
        }
        await this._todoListService.DeleteListAsync(id);
        return this.RedirectToAction(nameof(Index));
    }
}
