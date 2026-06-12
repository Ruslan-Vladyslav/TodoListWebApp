using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.TodoList;
using TodoListApp.WebApi.Models.Models.User;
using TodoListApp.WebApp.Controllers;
using TodoListApp.WebApp.Models;

namespace TodoListApp.Tests.Controllers;

public class TodoListShareControllerTests
{
    private const string OwnerUserId = "owner-user";
    private const string OtherUserId = "other-user";
    private const string ReceiverUserId = "receiver-user";

    private readonly Mock<IInvitationService> invitationService = new(MockBehavior.Strict);
    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);
    private readonly Mock<ITodoListService> todoListService = new(MockBehavior.Strict);
    private readonly Mock<IUserService> userService = new(MockBehavior.Strict);

    [Fact]
    public async Task ShareGetWhenCurrentUserIsNotOwnerReturnsForbid()
    {
        var controller = this.CreateController(OtherUserId);

        this.todoListService
            .Setup(x => x.GetByIdListAsync(42))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 42,
                Title = "Shared plan",
                UserId = OwnerUserId,
            });

        var result = await controller.Share(42);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task SharePostWhenReceiverIsAlreadyMemberAddsModelErrorAndDoesNotSendInvitation()
    {
        var controller = this.CreateController(OwnerUserId);
        var model = new ShareTodoListViewModel
        {
            ListId = 7,
            ReceiverInput = "member@example.test",
            Role = nameof(TodoListRole.Editor),
        };

        this.todoListService
            .Setup(x => x.GetByIdListAsync(model.ListId))
            .ReturnsAsync(new ModelTodoList
            {
                Id = model.ListId,
                Title = "House tasks",
                Description = "Shared chores",
                UserId = OwnerUserId,
            });

        this.userService
            .Setup(x => x.GetByEmailAsync(model.ReceiverInput))
            .ReturnsAsync(new UserModel
            {
                Id = ReceiverUserId,
                UserName = "receiver",
                Email = model.ReceiverInput,
            });

        this.accessService
            .Setup(x => x.GetUserRoleAsync(ReceiverUserId, model.ListId))
            .ReturnsAsync(TodoListRole.Viewer);

        var result = await controller.Share(model);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().BeSameAs(model);
        controller.ModelState[nameof(ShareTodoListViewModel.ReceiverInput)]!
            .Errors
            .Should()
            .ContainSingle(error => error.ErrorMessage == "This user is already a member of this list.");

        this.invitationService.Verify(
            x => x.SendInvitationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TodoListRole>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task SharePostWhenInvitationIsAlreadyPendingAddsModelErrorAndDoesNotSendInvitation()
    {
        var controller = this.CreateController(OwnerUserId);
        var model = new ShareTodoListViewModel
        {
            ListId = 7,
            ReceiverInput = "receiver-name",
            Role = nameof(TodoListRole.Editor),
        };

        this.todoListService
            .Setup(x => x.GetByIdListAsync(model.ListId))
            .ReturnsAsync(new ModelTodoList
            {
                Id = model.ListId,
                Title = "House tasks",
                UserId = OwnerUserId,
            });

        this.userService
            .Setup(x => x.GetByEmailAsync(model.ReceiverInput))
            .ReturnsAsync((UserModel?)null);

        this.userService
            .Setup(x => x.GetByUserNameAsync(model.ReceiverInput))
            .ReturnsAsync(new UserModel
            {
                Id = ReceiverUserId,
                UserName = model.ReceiverInput,
            });

        this.accessService
            .Setup(x => x.GetUserRoleAsync(ReceiverUserId, model.ListId))
            .ReturnsAsync((TodoListRole?)null);

        this.invitationService
            .Setup(x => x.HasPendingInvitationAsync(model.ListId, ReceiverUserId))
            .ReturnsAsync(true);

        var result = await controller.Share(model);

        result.Should().BeOfType<ViewResult>();
        controller.ModelState[nameof(ShareTodoListViewModel.ReceiverInput)]!
            .Errors
            .Should()
            .ContainSingle(error => error.ErrorMessage == "An invitation has already been sent to this user.");

        this.invitationService.Verify(
            x => x.SendInvitationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<TodoListRole>(),
                It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task RevokeWhenTargetIsOwnerReturnsBadRequestAndDoesNotRevokeAccess()
    {
        var controller = this.CreateController(OwnerUserId);

        this.todoListService
            .Setup(x => x.GetByIdListAsync(11))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 11,
                Title = "Owner list",
                UserId = OwnerUserId,
            });

        var result = await controller.Revoke(11, OwnerUserId);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Owner cannot be removed");
        this.accessService.Verify(
            x => x.RevokeAccessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task ChangeRoleWhenOwnerChangesMemberRoleUpdatesRoleAndRedirectsToMembers()
    {
        var controller = this.CreateController(OwnerUserId);

        this.todoListService
            .Setup(x => x.GetByIdListAsync(11))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 11,
                Title = "Owner list",
                UserId = OwnerUserId,
            });

        this.accessService
            .Setup(x => x.UpdateRoleAsync(OwnerUserId, ReceiverUserId, 11, TodoListRole.Editor))
            .Returns(Task.CompletedTask);

        var result = await controller.ChangeRole(11, ReceiverUserId, TodoListRole.Editor);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Members");
        redirect.RouteValues!["id"].Should().Be(11);
        this.accessService.VerifyAll();
    }

    private TodoListShareController CreateController(string currentUserId)
    {
        var controller = new TodoListShareController(
            this.invitationService.Object,
            this.accessService.Object,
            this.todoListService.Object,
            this.userService.Object);

        controller.SignInAs(currentUserId);
        return controller;
    }
}
