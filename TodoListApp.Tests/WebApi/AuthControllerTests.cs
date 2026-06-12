using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.Auth;

namespace TodoListApp.Tests.WebApi;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> authService = new(MockBehavior.Strict);

    [Fact]
    public async Task RegisterWhenSuccessfulReturnsOk()
    {
        var controller = this.CreateController();
        var request = new UserRegisterRequest
        {
            Email = "user@example.test",
            Password = "password1",
            ConfirmPassword = "password1",
        };

        var response = new AuthResponse
        {
            IsSuccessful = true,
            Token = "token",
        };

        this.authService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(response);

        var result = await controller.Register(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(response);
    }

    [Fact]
    public async Task RegisterWhenFailedReturnsBadRequest()
    {
        var controller = this.CreateController();
        var request = new UserRegisterRequest
        {
            Email = "user@example.test",
            Password = "password1",
            ConfirmPassword = "password1",
        };

        this.authService
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(new AuthResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Registration failed",
            });

        var result = await controller.Register(request);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Registration failed");
    }

    [Fact]
    public async Task LoginWhenFailedReturnsUnauthorized()
    {
        var controller = this.CreateController();
        var request = new UserLoginRequest
        {
            Email = "user@example.test",
            Password = "bad-password",
        };

        this.authService
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(new AuthResponse
            {
                IsSuccessful = false,
                ErrorMessage = "Invalid login",
            });

        var result = await controller.Login(request);

        var unauthorized = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorized.Value.Should().Be("Invalid login");
    }

    [Fact]
    public async Task ChangePasswordWhenServiceReturnsFalseReturnsBadRequest()
    {
        var controller = this.CreateController();
        var request = new ChangePasswordRequest
        {
            UserId = "user-id",
            CurrentPassword = "old-password",
            NewPassword = "new-password",
        };

        this.authService
            .Setup(x => x.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword))
            .ReturnsAsync(false);

        var result = await controller.ChangePassword(request);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task GenerateResetTokenWhenTokenMissingReturnsNotFound()
    {
        var controller = this.CreateController();
        var request = new ResetTokenRequest
        {
            Email = "missing@example.test",
        };

        this.authService
            .Setup(x => x.GeneratePasswordResetTokenAsync(request.Email))
            .ReturnsAsync((string?)null);

        var result = await controller.GenerateResetToken(request);

        result.Should().BeOfType<NotFoundResult>();
    }

    private AuthController CreateController()
    {
        return new AuthController(this.authService.Object);
    }
}
