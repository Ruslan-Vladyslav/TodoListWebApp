using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoListController : Controller
{
    private readonly ITodoListService _todoListService;
    private readonly int pageSize = 8;

    public TodoListController(ITodoListService todoListService)
    {
        this._todoListService = todoListService;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View("Error");
        }

        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
        {
            return this.Challenge();
        }

        var allLists = await this._todoListService.GetAllListByUserAsync(1, int.MaxValue, userName);

        int totalLists = allLists.Count();

        var pagedLists = allLists
            .Skip((page - 1) * this.pageSize)
            .Take(this.pageSize)
            .ToList();

        this.ViewBag.CurrentPage = page;
        this.ViewBag.TotalPages = (int)Math.Ceiling(totalLists / (double)this.pageSize);

        return this.View(pagedLists);
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
            return this.NotFound();
        }
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

        model.UserId = User.Identity?.Name;

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
            UserId = list.UserId,
        };

        return this.View(updateModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTodoList model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        await this._todoListService.UpdateListAsync(id, model);
        return this.RedirectToAction(nameof(Index));
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
