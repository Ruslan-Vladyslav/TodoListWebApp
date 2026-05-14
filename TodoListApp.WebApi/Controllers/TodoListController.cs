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
    public async Task<IActionResult> GetAllLists([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var lists = await this._todoListService.GetAllListAsync(page, pageSize);
        return this.Ok(lists);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAllListsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var lists = await this._todoListService.GetAllListByUserAsync(page, pageSize, userId);
        return this.Ok(lists);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetListById(int id)
    {
        var list = await this._todoListService.GetByIdListAsync(id);
        return this.Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> CreateList([FromBody] CreateTodoList model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        var created = await this._todoListService.CreateListAsync(model);
        return this.Ok(created);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateList(int id, [FromBody] UpdateTodoList model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(this.ModelState);
        }

        await this._todoListService.UpdateListAsync(id, model);
        return this.Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteList(int id)
    {
        await this._todoListService.DeleteListAsync(id);
        return this.Ok();
    }
}
