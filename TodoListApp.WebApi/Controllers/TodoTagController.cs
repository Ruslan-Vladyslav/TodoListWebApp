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
    public async Task<IActionResult> GetAllTags(int page = 1, int pageSize = 10)
    {
        var tags = await this._service.GetAllTagsAsync(page, pageSize);
        return this.Ok(tags);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTagById(int id)
    {
        var tag = await this._service.GetByIdTagAsync(id);
        return this.Ok(tag);
    }
    [HttpPost("{tagId}/addTask/{taskId}")]
    public async Task<IActionResult> AddTagToTask(int tagId, int taskId)
    {
        await this._service.AddTagToTaskAsync(taskId, tagId);
        return this.Ok();
    }


    [HttpDelete("{tagId}/removeTask/{taskId}")]
    public async Task<IActionResult> DeleteTagFromTask(int taskId, int tagId)
    {
        await this._service.DeleteTagFromTaskAsync(taskId, tagId);
        return this.NoContent();
    }

    [HttpGet("{tagId:int}/tasks")]
    public async Task<IActionResult> GetTasksByTag(int tagId)
    {
        var tasks = await this._service.GetTasksByTagAsync(tagId);
        return this.Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] ModelTodoTag newTag)
    {
        var createdTag = await this._service.CreateTagAsync(newTag?.Name!);
        return this.Ok(createdTag);
    }

}
