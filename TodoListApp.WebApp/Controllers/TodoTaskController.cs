using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;
using TodoListApp.WebApi.Models.Models.TodoTask;
using TodoListApp.WebApp.Models;

namespace TodoListApp.WebApp.Controllers;

[Authorize]
public class TodoTaskController : Controller
{
    private readonly ITodoTaskService _todoTaskService;
    private readonly ITodoListService _todoListService;
    private readonly ITodoTagService _todoTagService;
    private readonly ITodoCommentService _todoCommentService;
    private readonly UserManager<IdentityUser> _userManager;

    private readonly int pageSize = 6;

    public TodoTaskController(
        ITodoTaskService todoTaskService,
        ITodoListService todoListService,
        ITodoTagService todoTagService,
        ITodoCommentService todoCommentService,
        UserManager<IdentityUser> userManager)
    {
        this._todoTaskService = todoTaskService;
        this._todoListService = todoListService;
        this._todoTagService = todoTagService;
        this._todoCommentService = todoCommentService;
        this._userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1, string? sortBy = null, int? statusId = null)
    {
        TodoTaskStatus? statusFilter = null;
        if (statusId.HasValue)
        {
            statusFilter = (TodoTaskStatus)statusId.Value;
        }

        var UserName = User.Identity?.Name;
        var allTasks = await this._todoTaskService.GetAllTasksAsync(1, int.MaxValue, null, UserName, statusFilter, sortBy);

        var pagedTasks = allTasks
            .Skip((page - 1) * this.pageSize)
            .Take(this.pageSize)
            .ToList();

        this.ViewBag.SortOptions = new SelectList(
            new List<SelectListItem>
            {
            new SelectListItem { Text = "Title (A - Z)", Value = "asc(title)" },
            new SelectListItem { Text = "Title (Z - A)", Value = "desc(title)" },
            new SelectListItem { Text = "Due Date ↑", Value = "asc(duedate)" },
            new SelectListItem { Text = "Due Date ↓", Value = "desc(duedate)" },
            },
            "Value",
            "Text",
            sortBy
        );

        this.ViewBag.StatusFilter = new SelectList(
            Enum.GetValues(typeof(TodoTaskStatus))
                .Cast<TodoTaskStatus>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = Regex.Replace(e.ToString(), "(\\B[A-Z])", " $1")
                }),
            "Id",
            "Name",
            statusId
        );
        this.ViewBag.Page = page;
        this.ViewBag.SortBy = sortBy;
        this.ViewBag.StatusId = statusId;
        this.ViewBag.TotalPages = (int)Math.Ceiling(allTasks.Count() / (double)this.pageSize);
        this.ViewBag.CurrentPage = page;

        return this.View(pagedTasks);
    }


    public async Task<IActionResult> Details(int id)
    {
        var task = await this._todoTaskService.GetByIdTaskAsync(id);

        var UserName = User.Identity?.Name;
        task.AssignedUserId = UserName;
        task.UserId = UserName!;

        if (task == null)
        {
            return this.NotFound();
        }
        return this.View(task);
    }

    public async Task<IActionResult> Create(int? todoListId)
    {
        var UserName = User.Identity?.Name;

        this.ViewBag.ToDoLists = new SelectList(await this._todoListService.GetAllListByUserAsync(1, 100, UserName), "Id", "Title");

        var model = new CreateTodoTask();

        if (todoListId.HasValue)
        {
            model.TodoListId = todoListId.Value;
        }

        model.UserId = UserName;
        model.AssignedUserId = UserName;

        return this.View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoTask model)
    {
        var UserName = User.Identity?.Name;

        if (!this.ModelState.IsValid)
        {
            this.ViewBag.ToDoLists = new SelectList(await this._todoListService.GetAllListByUserAsync(1, 100, UserName!), "Id", "Title");

            foreach (var error in this.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return this.View(model);
        }

        model.UserId = UserName!;
        model.AssignedUserId = UserName;

        _ = await this._todoTaskService.CreateTaskAsync(model);
        return this.RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var task = await this._todoTaskService.GetByIdTaskAsync(id);

        if (task == null)
        {
            return this.NotFound();
        }

        if (task.UserId != User.Identity!.Name)
        {
            return this.Forbid();
        }

        var UserName = User.Identity?.Name;

        this.ViewBag.ToDoLists = new SelectList(await this._todoListService.GetAllListByUserAsync(1, 10, UserName!), "Id", "Title", task.TodoListId);
        this.ViewBag.Tags = new SelectList(await this._todoTagService.GetAllTagsAsync(1, 10), "Id", "Name");
        this.ViewBag.Statuses = new SelectList(
            Enum.GetValues(typeof(TodoTaskStatus))
                .Cast<TodoTaskStatus>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = Regex.Replace(e.ToString(), "(\\B[A-Z])", " $1")
                }),
            "Id",
            "Name",
            task.Status
        );

        var model = new UpdateTodoTask
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate.ToLocalTime(),
            Status = task.Status,
            UserId = task.UserId,
            AssignedUserId = task.AssignedUserId,
            TodoListId = task.TodoListId,
            Tags = task.Tags,
            Comments = task.Comments
        };

        var allUsers = await this._userManager.Users.ToListAsync();
        this.ViewBag.Users = new SelectList(allUsers, "Id", "UserName", model.AssignedUserId);

        return this.View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTodoTask model)
    {
        if (!this.ModelState.IsValid)
        {
            this.ViewBag.ToDoLists = new SelectList(await this._todoListService.GetAllListAsync(1, 100), "Id", "Title", model.TodoListId);
            this.ViewBag.Tags = new SelectList(await this._todoTagService.GetAllTagsAsync(1, 100), "Id", "Name");
            this.ViewBag.Statuses = new SelectList(
                Enum.GetValues(typeof(TodoTaskStatus))
                    .Cast<TodoTaskStatus>()
                    .Select(e => new
                    {
                        Id = (int)e,
                        Name = Regex.Replace(e.ToString(), "(\\B[A-Z])", " $1")
                    }),
                "Id",
                "Name",
                model.Status
            );
            foreach (var error in this.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return this.View(model);
        }

        model.DueDate = model.DueDate.ToUniversalTime();
        await this._todoTaskService.UpdateTaskAsync(id, model);
        return this.RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddTagToTask(int taskId, int tagId)
    {
        if (!this.ModelState.IsValid)
        {
            foreach (var error in this.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        await this._todoTagService.AddTagToTaskAsync(taskId, tagId);
        return this.RedirectToAction("Edit", new { id = taskId });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAndAddTag(int taskId, string newTagName)
    {
        if (!this.ModelState.IsValid)
        {
            foreach (var error in this.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        if (string.IsNullOrWhiteSpace(newTagName))
        {
            return this.RedirectToAction("Edit", new { id = taskId });
        }

        var tag = await this._todoTagService.CreateTagAsync(newTagName);
        await this._todoTagService.AddTagToTaskAsync(taskId, tag.Id);

        return this.RedirectToAction("Edit", new { id = taskId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTagFromTask(int taskId, int tagId)
    {
        if (!this.ModelState.IsValid)
        {
            foreach (var error in this.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
        }

        await this._todoTagService.DeleteTagFromTaskAsync(taskId, tagId);
        return this.RedirectToAction("Edit", new { id = taskId });
    }

    public async Task<IActionResult> DeleteConfirm(int id)
    {
        var task = await this._todoTaskService.GetByIdTaskAsync(id);
        if (task == null)
        {
            return this.NotFound();
        }

        if (task.UserId != User.Identity!.Name)
        {
            return this.Forbid();
        }

        return this.View(task);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await this._todoTaskService.DeleteTaskAsync(id);
        return this.RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        string searchType = "Title",
        string? title = null,
        DateTime? createDate = null,
        DateTime? dueDate = null,
        int? tagId = null,
        int page = 1)
        {
        var userName = User.Identity!.Name;
        var pageSize = 6;

        var tags = await _todoTagService.GetAllTagsAsync(1, pageSize);

        IEnumerable<ModelTodoTask> allResults = Enumerable.Empty<ModelTodoTask>();

        ViewBag.SearchTypeOptions = new SelectList(new[]
        {
            new SelectListItem { Text = "Title", Value = "Title" },
            new SelectListItem { Text = "Creation Date", Value = "CreationDate" },
            new SelectListItem { Text = "Due Date", Value = "DueDate" },
            new SelectListItem { Text = "Tag", Value = "Tag" }
        }, "Value", "Text", searchType);

        switch (searchType)
        {
            case "Title":
                if (!string.IsNullOrWhiteSpace(title))
                {
                    allResults = await _todoTaskService.GetAllTasksByTitleAsync(1, int.MaxValue, userName, title);
                }

                break;

            case "CreationDate":
                if (createDate.HasValue)
                {
                    allResults = await _todoTaskService.GetAllTasksByCreateDateAsync(1, int.MaxValue, userName, createDate.Value);
                }

                break;

            case "DueDate":
                if (dueDate.HasValue)
                {
                    allResults = await _todoTaskService.GetAllTasksByDueDateAsync(1, int.MaxValue, userName, dueDate.Value);
                }

                break;

            case "Tag":
                if (tagId.HasValue)
                {
                    allResults = await _todoTagService.GetTasksByTagAsync(tagId.Value);
                }

                break;
            default:
                break;
        }

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(allResults.Count() / (double)pageSize);

        ViewBag.SearchType = searchType;
        ViewBag.Title = title;
        ViewBag.CreateDate = createDate;
        ViewBag.DueDate = dueDate;
        ViewBag.TagId = tagId;

        ViewBag.Tags = new SelectList(tags, "Id", "Name");

        var paged = allResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(allResults.Count() / (double)pageSize);

        var vm = new TasksPartialViewModel
        {
            Tasks = paged,
            CurrentPage = page,
            TotalPages = totalPages,
            SearchType = searchType,
            TagId = tagId,
            Title = title,
            CreateDate = createDate,
            DueDate = dueDate,
            Action = "Search",
            Controller = "TodoTask",
        };

        return View(vm);
    }

    public async Task<ActionResult> TasksByList(int todoListId, int page = 1, string? sortBy = null)
    {
        var UserName = User.Identity!.Name;

        if (!this.ModelState.IsValid)
        {
            return this.View("Error");
        }

        var allTasks = await this._todoTaskService.GetAllTasksAsync(1, int.MaxValue, todoListId, UserName, null, sortBy);

        int totalTasks = allTasks.Count();
        var pagedResults = allTasks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalTasks / (double)pageSize);
        ViewBag.TodoListId = todoListId;

        var todoList = await this._todoListService.GetByIdListAsync(todoListId);
        ViewBag.TodoListName = todoList.Title!;

        return this.View(pagedResults);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCommentToTask(int taskId, string commentContent)
    {
        if (string.IsNullOrWhiteSpace(commentContent))
        {
            return this.RedirectToAction("Edit", new { id = taskId });
        }

        var comment = new CreateTodoComment
        {
            Text = commentContent
        };

        _ = await this._todoCommentService.CreateCommentAsync(taskId, comment);

        return this.RedirectToAction("Edit", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCommentFromTask(int taskId, int commentId)
    {
        await this._todoCommentService.DeleteCommentAsync(commentId);
        return this.RedirectToAction("Edit", new { id = taskId });
    }
}
