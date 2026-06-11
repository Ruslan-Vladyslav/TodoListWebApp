using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Common;
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
    private readonly IAccessService _accessService;
    private readonly IUserService _userService;

    private readonly int pageSize = 6;

    public TodoTaskController(
        ITodoTaskService todoTaskService,
        ITodoListService todoListService,
        ITodoTagService todoTagService,
        ITodoCommentService todoCommentService,
        IAccessService accessService,
        IUserService userService)
    {
        this._todoTaskService = todoTaskService;
        this._todoListService = todoListService;
        this._todoTagService = todoTagService;
        this._todoCommentService = todoCommentService;
        this._accessService = accessService;
        this._userService = userService;

    }

    public async Task<IActionResult> Index(int page = 1, string? sortBy = null, int? statusId = null)
    {
        TodoTaskStatus? statusFilter = null;
        if (statusId.HasValue)
        {
            statusFilter = (TodoTaskStatus)statusId.Value;
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var pagedTasks = await _todoTaskService.GetAllTasksAsync(
            page,
            pageSize,
            null,
            userId,
            statusFilter,
            sortBy);

        var userIds = pagedTasks.Items
            .Where(x => !string.IsNullOrEmpty(x.UserId))
            .Select(x => x.UserId!)
            .Concat(pagedTasks.Items
                .Where(x => !string.IsNullOrEmpty(x.AssignedUserId))
                .Select(x => x.AssignedUserId!))
            .Distinct()
            .ToList();

        var users = await _userService.GetUsersByIdsAsync(userIds);

        foreach (var task in pagedTasks.Items)
        {
            task.UserName =
                users.GetValueOrDefault(task.UserId!);

            if (!string.IsNullOrEmpty(task.AssignedUserId))
            {
                task.AssignedUserName = users.GetValueOrDefault(task.AssignedUserId!);
            }

            task.Role =
                await _accessService.GetUserRoleAsync(
                    userId!,
                    task.TodoListId) ?? TodoListRole.Viewer;

            task.IsShared = task.ListOwnerId != userId;
        }

        ViewBag.SortOptions = new SelectList(
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

        var statuses = Enum.GetValues(typeof(TodoTaskStatus))
            .Cast<TodoTaskStatus>()
            .Select(e => new
            {
                Id = (int?)e,
                Name = Regex.Replace(e.ToString(), "(\\B[A-Z])", " $1")
            })
            .ToList();

        statuses.Insert(0, new
        {
            Id = (int?)null,
            Name = "-- All statuses --"
        });

        ViewBag.StatusFilter = new SelectList(statuses, "Id", "Name", statusId);

        ViewBag.Page = page;
        ViewBag.SortBy = sortBy;
        ViewBag.StatusId = statusId;

        ViewBag.TotalPages = (int)Math.Ceiling(pagedTasks.TotalCount / (double)pageSize);
        ViewBag.CurrentPage = page;

        return View(pagedTasks.Items.ToList());
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _todoTaskService.GetByIdTaskAsync(id, userId!);
        if (task == null)
        {
            return NotFound();
        }

        var role = await _accessService.GetUserRoleAsync(userId!, task.TodoListId);

        if (!string.IsNullOrEmpty(task.UserId))
        {
            var user = await _userService.GetByIdAsync(task.UserId);

            task.UserName = user?.UserName;
        }

        if (!string.IsNullOrEmpty(task.AssignedUserId))
        {
            var assignedUser = await _userService.GetByIdAsync(task.AssignedUserId);
            task.AssignedUserName = assignedUser?.UserName;
        }

        if (task.Comments != null && task.Comments.Any())
        {
            var commentUserIds = task.Comments.Select(c => c.UserId).Distinct().ToList();
            var commentUsers = await _userService.GetUsersByIdsAsync(commentUserIds);
            foreach (var comment in task.Comments)
            {
                comment.UserName = commentUsers.GetValueOrDefault(comment.UserId);
            }
        }

        ViewBag.Role = role;
        return View(task);
    }

    public async Task<IActionResult> Create(int? todoListId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        this.ViewBag.ToDoLists = new SelectList((await this._todoListService.GetAllListByUserAsync(1, 100, userId)).Items, "Id", "Title");

        var model = new CreateTodoTask();

        if (todoListId.HasValue)
        {
            model.TodoListId = todoListId.Value;
        }

        model.UserId = userId;
        model.AssignedUserId = userId;

        ViewBag.Role = model.TodoListId > 0
            ? await _accessService.GetUserRoleAsync(userId!, model.TodoListId)
            : null;

        return this.View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTodoTask model)
    {
        var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!this.ModelState.IsValid)
        {
            this.ViewBag.ToDoLists = new SelectList((await this._todoListService.GetAllListByUserAsync(1, 100, UserId!)).Items, "Id", "Title");
            ViewBag.Role = model.TodoListId > 0
                ? await _accessService.GetUserRoleAsync(UserId!, model.TodoListId)
                : null;

            foreach (var error in this.ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return this.View(model);
        }

        model.UserId = UserId!;
        model.AssignedUserId = UserId;

        _ = await this._todoTaskService.CreateTaskAsync(model);
        return this.RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _todoTaskService.GetByIdTaskAsync(id, userId!);
        if (task == null)
        {
            return NotFound();
        }

        var role = await _accessService.GetUserRoleAsync(userId!, task.TodoListId);

        if (role == TodoListRole.Viewer)
        {
            return Forbid();
        }

        ViewBag.Role = role;

        var ownerUser = string.IsNullOrEmpty(task.UserId)
         ? null
         : await _userService.GetByIdAsync(task.UserId);

        var assignedUser = !string.IsNullOrEmpty(task.AssignedUserId)
             ? await _userService.GetByIdAsync(task.AssignedUserId)
             : null;

        ViewBag.ToDoLists = new SelectList(
            (await _todoListService.GetAllListByUserAsync(1, 10, userId!)).Items,
            "Id", "Title", task.TodoListId);

        ViewBag.Tags = new SelectList(
            (await this._todoTagService.GetAllTagsAsync(1, 10)).Items,
            "Id", "Name");

        ViewBag.Statuses = new SelectList(
            Enum.GetValues(typeof(TodoTaskStatus))
                .Cast<TodoTaskStatus>()
                .Select(e => new
                {
                    Id = (int)e,
                    Name = Regex.Replace(e.ToString(), "(\\B[A-Z])", " $1")
                }),
            "Id",
            "Name",
            task.Status);

        var model = new UpdateTodoTask
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Status = task.Status,
            UserId = task.UserId,
            UserName = ownerUser?.UserName,
            AssignedUserId = task.AssignedUserId,
            AssignedUserName = assignedUser?.UserName,
            TodoListId = task.TodoListId,
            Tags = task.Tags,
            Comments = task.Comments
        };

        var list = await _todoListService.GetByIdListAsync(task.TodoListId);
        var accesses = await _accessService.GetAccessListAsync(task.TodoListId);
        var userIds = accesses.Select(x => x.TargetUserId).ToList();
        if (!string.IsNullOrEmpty(list?.UserId))
        {
            userIds.Add(list.UserId);
        }

        var allUsers = await _userService.GetAllAsync();
        var members = allUsers.Where(u => userIds.Contains(u.Id)).ToList();

        ViewBag.Users = new SelectList(
            members,
            "Id",
            "UserName",
            model.AssignedUserId);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateTodoTask model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var role = await _accessService.GetUserRoleAsync(userId!, model.TodoListId);

        if (role == TodoListRole.Viewer)
        {
            return Forbid();
        }

        await _todoTaskService.UpdateTaskAsync(id, model, userId!);
        return RedirectToAction(nameof(Index));
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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._todoTagService.AddTagToTaskAsync(taskId, tagId, userId);
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

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tag = await this._todoTagService.CreateTagAsync(newTagName);
        await this._todoTagService.AddTagToTaskAsync(taskId, tag.Id, userId);

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
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._todoTagService.DeleteTagFromTaskAsync(taskId, tagId, userId);
        return this.RedirectToAction("Edit", new { id = taskId });
    }

    public async Task<IActionResult> DeleteConfirm(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _todoTaskService.GetByIdTaskAsync(id, userId!);
        if (task == null)
        {
            return NotFound();
        }

        var role = await _accessService.GetUserRoleAsync(userId!, task.TodoListId);

        if (role != TodoListRole.Owner)
        {
            return Forbid();
        }

        return View(task);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var task = await _todoTaskService.GetByIdTaskAsync(id, userId!);

        var role = await _accessService.GetUserRoleAsync(userId!, task.TodoListId);

        if (role != TodoListRole.Owner)
        {
            return Forbid();
        }

        await _todoTaskService.DeleteTaskAsync(id, userId!);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        string searchType = "Title",
        string? title = null,
        DateTime? createDate = null,
        DateTime? dueDate = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? tagId = null,
        int page = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var pageSize = 6;

        var tags = await _todoTagService.GetAllTagsAsync(1, 100);

        PagedResponse<ModelTodoTask>? pagedResponse = null;
        IEnumerable<ModelTodoTask>? tagResults = null;

        ViewBag.SearchTypeOptions = new SelectList(new[]
        {
            new SelectListItem { Text = "Title", Value = "Title" },
            new SelectListItem { Text = "Creation Date", Value = "CreationDate" },
            new SelectListItem { Text = "Due Date", Value = "DueDate" },
            new SelectListItem { Text = "Tag", Value = "Tag" },
            new SelectListItem { Text = "Date Range", Value = "DateRange" }
        }, "Value", "Text", searchType);

        switch (searchType)
        {
            case "Title":
                if (!string.IsNullOrWhiteSpace(title))
                {
                    pagedResponse = await _todoTaskService.GetAllTasksByTitleAsync(page, pageSize, userId, title);
                }
                break;

            case "CreationDate":
                if (createDate.HasValue)
                {
                    pagedResponse = await _todoTaskService.GetAllTasksByCreateDateAsync(page, pageSize, userId, createDate.Value);
                }
                break;

            case "DueDate":
                if (dueDate.HasValue)
                {
                    pagedResponse = await _todoTaskService.GetAllTasksByDueDateAsync(page, pageSize, userId, dueDate.Value);
                }
                break;

            case "DateRange":
                if (fromDate.HasValue && toDate.HasValue)
                {
                    pagedResponse = await _todoTaskService.GetAllTasksByDateRangeAsync(
                            page,
                            pageSize,
                            userId,
                            fromDate.Value,
                            toDate.Value);
                }
                break;

            case "Tag":
                if (tagId.HasValue)
                {
                    tagResults = await _todoTagService.GetTasksByTagAsync(tagId.Value, userId!);
                }
                break;
            default:
                break;
        }

        int totalCount = 0;
        List<ModelTodoTask> pagedItems;

        if (pagedResponse != null)
        {
            totalCount = pagedResponse.TotalCount;
            pagedItems = pagedResponse.Items.ToList();
        }
        else if (tagResults != null)
        {
            totalCount = tagResults.Count();
            pagedItems = tagResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        else
        {
            pagedItems = new List<ModelTodoTask>();
        }

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        ViewBag.SearchType = searchType;
        ViewBag.Title = title;
        ViewBag.CreateDate = createDate;
        ViewBag.DueDate = dueDate;
        ViewBag.TagId = tagId;

        ViewBag.Tags = new SelectList(tags.Items, "Id", "Name");

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var vm = new TasksPartialViewModel
        {
            Tasks = pagedItems,
            CurrentPage = page,
            TotalPages = totalPages,
            SearchType = searchType,
            TagId = tagId,
            Title = title,
            CreateDate = createDate,
            DueDate = dueDate,
            FromDate = fromDate,
            ToDate = toDate,
            Action = "Search",
            Controller = "TodoTask",
        };

        return View(vm);
    }

    public async Task<ActionResult> TasksByList(int todoListId, int page = 1, string? sortBy = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var role = await _accessService.GetUserRoleAsync(userId!, todoListId);

        if (role == null)
        {
            return Forbid();
        }

        var pagedTasks = await _todoTaskService.GetAllTasksAsync(
            page,
            pageSize,
            todoListId,
            userId,
            null,
            sortBy);

        foreach (var task in pagedTasks.Items)
        {
            if (!string.IsNullOrEmpty(task.UserId))
            {
                var user = await _userService.GetByIdAsync(task.UserId);
                task.UserName = user?.UserName;
            }
            if (!string.IsNullOrEmpty(task.AssignedUserId))
            {
                var assignedUser = await _userService.GetByIdAsync(task.AssignedUserId);
                task.AssignedUserName = assignedUser?.UserName;
            }
        }

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(pagedTasks.TotalCount / (double)pageSize);
        ViewBag.TodoListId = todoListId;
        ViewBag.TodoListName = (await _todoListService.GetByIdListAsync(todoListId)).Title;
        ViewBag.Role = role;

        return View(pagedTasks.Items.ToList());
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCommentToTask(int taskId, string commentContent)
    {
        if (string.IsNullOrWhiteSpace(commentContent))
        {
            return this.RedirectToAction("Edit", new { id = taskId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var comment = new CreateTodoComment
        {
            Text = commentContent,
            UserId = userId!
        };

        _ = await this._todoCommentService.CreateCommentAsync(taskId, comment);

        return this.RedirectToAction("Edit", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCommentFromTask(int taskId, int commentId)
    {
        await this._todoCommentService.DeleteCommentAsync(commentId);
        return RedirectToAction("Edit", new { id = taskId });
    }

    [HttpGet]
    public async Task<IActionResult> GetListMembers(int listId)
    {
        var list = await _todoListService.GetByIdListAsync(listId);
        if (list == null)
        {
            return NotFound();
        }

        var accesses = await _accessService.GetAccessListAsync(listId);
        var userIds = accesses.Select(x => x.TargetUserId).ToList();
        if (!string.IsNullOrEmpty(list.UserId))
        {
            userIds.Add(list.UserId);
        }

        var allUsers = await _userService.GetAllAsync();
        var members = allUsers
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { id = u.Id, userName = u.UserName })
            .ToList();

        return Json(members);
    }
}
