using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTask;
using TodoListApp.WebApp.Controllers;

namespace TodoListApp.Tests.Controllers;

public class TodoTaskControllerTests
{
    private const string CurrentUserId = "current-user";
    private const string OwnerUserId = "owner-user";
    private const string AssignedUserId = "assigned-user";

    private readonly Mock<ITodoTaskService> todoTaskService = new(MockBehavior.Strict);
    private readonly Mock<ITodoListService> todoListService = new(MockBehavior.Strict);
    private readonly Mock<ITodoTagService> todoTagService = new(MockBehavior.Strict);
    private readonly Mock<ITodoCommentService> todoCommentService = new(MockBehavior.Strict);
    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);
    private readonly Mock<IUserService> userService = new(MockBehavior.Strict);

    [Fact]
    public async Task IndexMapsNamesRolesAndSharedFlagForReturnedTasks()
    {
        var controller = this.CreateController(CurrentUserId);
        var tasks = new List<ModelTodoTask>
        {
            new()
            {
                Id = 1,
                Title = "Shared task",
                UserId = OwnerUserId,
                AssignedUserId = AssignedUserId,
                TodoListId = 5,
                ListOwnerId = OwnerUserId,
                Status = TodoTaskStatus.InProgress,
            },
            new()
            {
                Id = 2,
                Title = "Own task",
                UserId = CurrentUserId,
                TodoListId = 6,
                ListOwnerId = CurrentUserId,
                Status = TodoTaskStatus.NotStarted,
            },
        };

        this.todoTaskService
            .Setup(x => x.GetAllTasksAsync(2, 6, null, CurrentUserId, TodoTaskStatus.Completed, "desc(title)"))
            .ReturnsAsync(new PagedResponse<ModelTodoTask>
            {
                Items = tasks,
                Page = 2,
                PageSize = 6,
                TotalCount = 13,
            });

        this.userService
            .Setup(x => x.GetUsersByIdsAsync(
                It.Is<List<string>>(ids =>
                    ids.Count == 3 &&
                    ids.Contains(OwnerUserId) &&
                    ids.Contains(AssignedUserId) &&
                    ids.Contains(CurrentUserId))))
            .ReturnsAsync(new Dictionary<string, string?>
            {
                [OwnerUserId] = "Owner",
                [AssignedUserId] = "Assigned",
                [CurrentUserId] = "Current",
            });

        this.accessService
            .Setup(x => x.GetUserRoleAsync(CurrentUserId, 5))
            .ReturnsAsync(TodoListRole.Editor);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(CurrentUserId, 6))
            .ReturnsAsync(TodoListRole.Owner);

        var result = await controller.Index(2, "desc(title)", (int)TodoTaskStatus.Completed);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeAssignableTo<List<ModelTodoTask>>().Subject;

        model[0].UserName.Should().Be("Owner");
        model[0].AssignedUserName.Should().Be("Assigned");
        model[0].Role.Should().Be(TodoListRole.Editor);
        model[0].IsShared.Should().BeTrue();

        model[1].UserName.Should().Be("Current");
        model[1].Role.Should().Be(TodoListRole.Owner);
        model[1].IsShared.Should().BeFalse();

        ((int)controller.ViewBag.TotalPages).Should().Be(3);
        ((int)controller.ViewBag.CurrentPage).Should().Be(2);
    }

    [Fact]
    public async Task EditPostWhenCurrentUserIsViewerReturnsForbidAndDoesNotUpdateTask()
    {
        var controller = this.CreateController(CurrentUserId);
        var model = new UpdateTodoTask
        {
            Id = 99,
            Title = "No edit",
            Description = "Viewers cannot edit",
            TodoListId = 5,
            Status = TodoTaskStatus.NotStarted,
        };

        this.accessService
            .Setup(x => x.GetUserRoleAsync(CurrentUserId, model.TodoListId))
            .ReturnsAsync(TodoListRole.Viewer);

        var result = await controller.Edit(model.Id, model);

        result.Should().BeOfType<ForbidResult>();
        this.todoTaskService.Verify(
            x => x.UpdateTaskAsync(It.IsAny<int>(), It.IsAny<UpdateTodoTask>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteWhenCurrentUserIsNotOwnerReturnsForbidAndDoesNotDeleteTask()
    {
        var controller = this.CreateController(CurrentUserId);

        this.todoTaskService
            .Setup(x => x.GetByIdTaskAsync(99, CurrentUserId))
            .ReturnsAsync(new ModelTodoTask
            {
                Id = 99,
                Title = "Protected task",
                TodoListId = 5,
            });

        this.accessService
            .Setup(x => x.GetUserRoleAsync(CurrentUserId, 5))
            .ReturnsAsync(TodoListRole.Editor);

        var result = await controller.Delete(99);

        result.Should().BeOfType<ForbidResult>();
        this.todoTaskService.Verify(
            x => x.DeleteTaskAsync(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    private TodoTaskController CreateController(string currentUserId)
    {
        var controller = new TodoTaskController(
            this.todoTaskService.Object,
            this.todoListService.Object,
            this.todoTagService.Object,
            this.todoCommentService.Object,
            this.accessService.Object,
            this.userService.Object);

        controller.SignInAs(currentUserId);
        return controller;
    }
}
