using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoCommentController : ControllerBase
{
    private readonly ITodoCommentService _service;

    public TodoCommentController(ITodoCommentService service)
    {
        this._service = service;
    }

    [HttpPost("task/{taskId:int}")]
    public async Task<IActionResult> CreateComment(int taskId, [FromBody] CreateTodoComment model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var created = await this._service.CreateCommentAsync(taskId, model);
        return this.Ok(created);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCommentById(int id)
    {
        var comment = await this._service.GetCommentByIdAsync(id);
        return this.Ok(comment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        await this._service.DeleteCommentAsync(id);
        return this.Ok();
    }
}
