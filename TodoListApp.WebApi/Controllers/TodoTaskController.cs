using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class TodoTaskController : ControllerBase
{
    private readonly ITodoTaskService _service;

    public TodoTaskController(ITodoTaskService service)
    {
        this._service = service;
    }

    [HttpGet("by-list/{listId:int}")]
    public async Task<IActionResult> GetByListId(int listId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await this._service.GetByListIdAsync(listId, userId!);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTodoTask model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        model.UserId = userId!;

        var created = await this._service.CreateTaskAsync(model);

        return this.CreatedAtAction(
            nameof(GetTaskById),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTodoTask model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = await this._service.GetByIdTaskAsync(id, userId!);

        if (existing == null)
        {
            return NotFound($"Task with id {id} not found");
        }

        await this._service.UpdateTaskAsync(id, model, userId!);

        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var task = await this._service.GetByIdTaskAsync(id, userId!);

        if (task == null)
        {
            return NotFound($"Task with id {id} not found");
        }

        return Ok(task);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 8,
        [FromQuery] int? listId = null,
        [FromQuery] TodoTaskStatus? status = null,
        [FromQuery] string? sort = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await this._service.GetAllTasksAsync(
            page,
            pageSize,
            listId,
            userId,
            status,
            sort);

        return Ok(tasks);
    }

    [HttpGet("by-create")]
    public async Task<IActionResult> GetTasksByCreateDate(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? date = null)
    {
        if (date == null)
        {
            return BadRequest("Create date is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await this._service.GetAllTasksByCreateDateAsync(
            page,
            pageSize,
            userId,
            date.Value);

        return Ok(tasks);
    }

    [HttpGet("by-due")]
    public async Task<IActionResult> GetTasksByDueDate(
       [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? date = null)
    {
        if (date == null)
        {
            return BadRequest("Due date is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await this._service.GetAllTasksByDueDateAsync(
            page,
            pageSize,
            userId,
            date.Value);

        return Ok(tasks);
    }

    [HttpGet("by-title")]
    public async Task<IActionResult> GetTasksByTitle(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? title = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest("Title is required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await this._service.GetAllTasksByTitleAsync(
            page,
            pageSize,
            userId,
            title);

        return Ok(tasks);
    }

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetTasksByDateRange(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        if (fromDate == null || toDate == null)
        {
            return BadRequest("Both dates are required.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var tasks = await this._service.GetAllTasksByDateRangeAsync(
            page,
            pageSize,
            userId,
            fromDate.Value,
            toDate.Value);

        return Ok(tasks);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existing = await this._service.GetByIdTaskAsync(id, userId!);

        if (existing == null)
        {
            return NotFound($"Task with id {id} not found");
        }

        await this._service.DeleteTaskAsync(id, userId!);

        return NoContent();
    }
}
