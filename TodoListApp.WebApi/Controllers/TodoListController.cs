using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoListController : ControllerBase
{
    private readonly ITodoListService _todoListService;

    public TodoListController(ITodoListService service)
    {
        this._todoListService = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLists([FromQuery] int page = 1, [FromQuery] int pageSize = 8)
    {
        var lists = await this._todoListService.GetAllListAsync(page, pageSize);
        return this.Ok(lists);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAllListsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 8)
    {
        var lists = await this._todoListService.GetAllListByUserAsync(page, pageSize, userId);
        return this.Ok(lists);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetListById(int id)
    {
        var list = await this._todoListService.GetByIdListAsync(id);

        if (list == null)
        {
            return NotFound($"TodoList with id {id} not found");
        }

        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] CreateTodoList model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var created = await this._todoListService.CreateListAsync(model);
        return this.CreatedAtAction(
           nameof(GetListById),
           new { id = created.Id },
           created);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateList(int id, [FromBody] UpdateTodoList model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var existing = await this._todoListService.GetByIdListAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await this._todoListService.UpdateListAsync(id, model);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteList(int id)
    {
        var existing = await this._todoListService.GetByIdListAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        await this._todoListService.DeleteListAsync(id);

        return NoContent();
    }
}
