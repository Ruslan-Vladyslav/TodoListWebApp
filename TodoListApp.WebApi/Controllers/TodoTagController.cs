using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoTag;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoTagController : ControllerBase
{
    private readonly ITodoTagService _service;

    public TodoTagController(ITodoTagService service)
    {
        this._service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var tags = await this._service.GetAllTagsAsync(page, pageSize);
        return Ok(tags);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTagById(int id)
    {
        var tag = await this._service.GetByIdTagAsync(id);

        if (tag == null)
        {
            return NotFound($"Tag with id {id} not found");
        }

        return Ok(tag);
    }

    [HttpPost("tasks/{taskId:int}/tags/{tagId:int}")]
    public async Task<IActionResult> AddTagToTask(int taskId, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._service.AddTagToTaskAsync(taskId, tagId, userId!);
        return NoContent();
    }


    [HttpDelete("tasks/{taskId:int}/tags/{tagId:int}")]
    public async Task<IActionResult> DeleteTagFromTask(int taskId, int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        await this._service.DeleteTagFromTaskAsync(taskId, tagId, userId!);
        return NoContent();
    }

    [HttpGet("{tagId:int}/tasks")]
    public async Task<IActionResult> GetTasksByTag(int tagId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var tasks = await _service.GetTasksByTagAsync(tagId, userId!);
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await this._service.CreateTagAsync(request.Name);

        return this.CreatedAtAction(
            nameof(GetTagById),
            new { id = created.Id },
            created);
    }
}
