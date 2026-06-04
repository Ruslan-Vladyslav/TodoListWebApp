using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Auth;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.IsSuccessful)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.IsSuccessful)
        {
            return Unauthorized(result.ErrorMessage);
        }

        return Ok(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var result = await _authService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword);

        return result ? Ok() : BadRequest();
    }

    [HttpPost("reset-token")]
    public async Task<IActionResult> GenerateResetToken([FromBody] string email)
    {
        var token = await _authService.GeneratePasswordResetTokenAsync(email);

        if (token == null)
        {
            return NotFound();
        }

        return Ok(token);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(AuthResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(
            request.Email,
            request.Token,
            request.NewPassword);

        return result ? Ok() : BadRequest();
    }
}
