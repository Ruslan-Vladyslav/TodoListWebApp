using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoTaskController : ControllerBase
{
    private readonly ITodoTaskService _service;

    public TodoTaskController(ITodoTaskService service)
    {
        this._service = service;
    }

    [HttpGet("by-list/{listId}")]
    public async Task<IActionResult> GetByListId(int listId)
    {
        var tasks = await this._service.GetByListIdAsync(listId);
        return this.Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTodoTask model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var created = await this._service.CreateTaskAsync(model);
        return this.Ok(created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTodoTask model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        await this._service.UpdateTaskAsync(id, model);
        return this.Ok();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var task = await this._service.GetByIdTaskAsync(id);
        return this.Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks(
        int page = 1,
        int pageSize = 10,
        int? listId = null,
        string? userId = null,
        TodoTaskStatus? status = null,
        string? sort = null)
    {
        var tasks = await this._service.GetAllTasksAsync(page, pageSize, listId, userId, status, sort);
        return this.Ok(tasks);
    }

    [HttpGet("by-create")]
    public async Task<IActionResult> GetTasksByCreateDate(
        int page = 1,
        int pageSize = 10,
        string? userId = null,
        DateTime? date = null)
    {
        if (date == null)
        {
            return this.BadRequest("Create date is required.");
        }

        var tasks = await this._service.GetAllTasksByCreateDateAsync(page, pageSize, userId, date.Value);
        return this.Ok(tasks);
    }

    [HttpGet("by-due")]
    public async Task<IActionResult> GetTasksByDueDate(
        int page = 1,
        int pageSize = 10,
        string? userId = null,
        DateTime? date = null)
    {
        if (date == null)
        {
            return this.BadRequest("Due date is required.");
        }

        var tasks = await this._service.GetAllTasksByDueDateAsync(page, pageSize, userId, date.Value);
        return this.Ok(tasks);
    }

    [HttpGet("by-title")]
    public async Task<IActionResult> GetTasksByTitle(
        int page = 1,
        int pageSize = 10,
        string? userId = null,
        string? title = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return this.BadRequest("Title is required.");
        }

        var tasks = await this._service.GetAllTasksByTitleAsync(page, pageSize, userId, title);
        return this.Ok(tasks);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        await this._service.DeleteTaskAsync(id);
        return this.Ok();
    }
}
