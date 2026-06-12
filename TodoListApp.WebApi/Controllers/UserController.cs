using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var result = await _userService.GetByEmailAsync(email);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("by-username/{userName}")]
    public async Task<IActionResult> GetByUserName(string userName)
    {
        var result = await _userService.GetByUserNameAsync(userName);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost("by-ids")]
    public async Task<IActionResult> GetByIds([FromBody] List<string> ids)
    {
        var users = await _userService.GetAllAsync();

        var result = users
            .Where(u => ids.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u.UserName);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _userService.DeleteAsync(id);
        return Ok();
    }

    [HttpPut("{id}/username")]
    public async Task<IActionResult> UpdateUserName(
    string id,
    [FromBody] string userName)
    {
        await _userService.UpdateUserNameAsync(id, userName);
        return Ok();
    }

    [HttpGet("exists/{userName}")]
    public async Task<IActionResult> UserNameExists(
    string userName)
    {
        return Ok(
            await _userService.UserNameExistsAsync(userName));
    }
}
