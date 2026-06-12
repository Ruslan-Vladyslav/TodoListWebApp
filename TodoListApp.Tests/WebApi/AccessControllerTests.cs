using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Controllers;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Access;

namespace TodoListApp.Tests.WebApi;

public class AccessControllerTests
{
    private const string OwnerUserId = "owner-user";
    private const string TargetUserId = "target-user";

    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);

    [Fact]
    public async Task GrantUsesAuthenticatedOwnerAndReturnsOk()
    {
        var controller = this.CreateController();

        this.accessService
            .Setup(x => x.GrantAccessAsync(OwnerUserId, TargetUserId, 15, TodoListRole.Editor))
            .Returns(Task.CompletedTask);

        var result = await controller.Grant(TargetUserId, 15, TodoListRole.Editor);

        result.Should().BeOfType<OkResult>();
        this.accessService.VerifyAll();
    }

    [Fact]
    public async Task RevokeUsesAuthenticatedOwnerAndReturnsNoContent()
    {
        var controller = this.CreateController();

        this.accessService
            .Setup(x => x.RevokeAccessAsync(OwnerUserId, TargetUserId, 15))
            .Returns(Task.CompletedTask);

        var result = await controller.Revoke(TargetUserId, 15);

        result.Should().BeOfType<NoContentResult>();
        this.accessService.VerifyAll();
    }

    [Fact]
    public async Task GetRoleReturnsRoleForAuthenticatedUser()
    {
        var controller = this.CreateController();

        this.accessService
            .Setup(x => x.GetUserRoleAsync(OwnerUserId, 15))
            .ReturnsAsync(TodoListRole.Owner);

        var result = await controller.GetRole(15);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(TodoListRole.Owner);
    }

    [Fact]
    public async Task GetMembersReturnsAccessItems()
    {
        var controller = this.CreateController();
        var members = new[]
        {
            new ModelTodoListAccess
            {
                TodoListId = 15,
                OwnerUserId = OwnerUserId,
                TargetUserId = TargetUserId,
                Role = TodoListRole.Viewer,
                Accepted = true,
            },
        };

        this.accessService
            .Setup(x => x.GetAccessListAsync(15))
            .ReturnsAsync(members);

        var result = await controller.GetMembers(15);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(members);
    }

    private AccessController CreateController()
    {
        var controller = new AccessController(this.accessService.Object);
        controller.SignInAs(OwnerUserId);
        return controller;
    }
}
