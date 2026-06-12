using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Models.User;

namespace TodoListApp.Tests.WebApi;

public class UserControllerTests
{
    private readonly Mock<IUserService> userService = new(MockBehavior.Strict);

    [Fact]
    public async Task GetByIdWhenUserMissingReturnsNotFound()
    {
        var controller = this.CreateController();

        this.userService
            .Setup(x => x.GetByIdAsync("missing-user"))
            .ReturnsAsync((UserModel?)null);

        var result = await controller.GetById("missing-user");

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByEmailWhenFoundReturnsUser()
    {
        var controller = this.CreateController();
        var user = new UserModel
        {
            Id = "user-id",
            UserName = "user",
            Email = "user@example.test",
        };

        this.userService
            .Setup(x => x.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        var result = await controller.GetByEmail(user.Email);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(user);
    }

    [Fact]
    public async Task GetByIdsFiltersUsersAndReturnsDictionary()
    {
        var controller = this.CreateController();

        this.userService
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<UserModel>
            {
                new()
                {
                    Id = "one",
                    UserName = "First",
                },
                new()
                {
                    Id = "two",
                    UserName = "Second",
                },
            });

        var result = await controller.GetByIds(new List<string> { "two" });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var users = ok.Value.Should().BeAssignableTo<Dictionary<string, string>>().Subject;
        users.Should().ContainSingle();
        users["two"].Should().Be("Second");
    }

    [Fact]
    public async Task UpdateUserNameInvokesServiceAndReturnsOk()
    {
        var controller = this.CreateController();

        this.userService
            .Setup(x => x.UpdateUserNameAsync("user-id", "new-name"))
            .Returns(Task.CompletedTask);

        var result = await controller.UpdateUserName("user-id", "new-name");

        result.Should().BeOfType<OkResult>();
        this.userService.VerifyAll();
    }

    private UserController CreateController()
    {
        return new UserController(this.userService.Object);
    }
}
