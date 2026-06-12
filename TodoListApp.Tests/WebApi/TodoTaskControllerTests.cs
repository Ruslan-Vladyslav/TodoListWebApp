using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Controllers;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Tests.WebApi;

public class TodoTaskControllerTests
{
    private const string CurrentUserId = "current-user";

    private readonly Mock<ITodoTaskService> todoTaskService = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateTaskWithInvalidModelReturnsBadRequestAndDoesNotCreate()
    {
        var controller = this.CreateController();
        controller.ModelState.AddModelError(nameof(CreateTodoTask.Title), "Title is required");

        var result = await controller.CreateTask(new CreateTodoTask());

        result.Should().BeOfType<BadRequestObjectResult>();
        this.todoTaskService.Verify(
            x => x.CreateTaskAsync(It.IsAny<CreateTodoTask>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTaskAssignsAuthenticatedUserAndReturnsCreatedAtAction()
    {
        var controller = this.CreateController();
        var request = new CreateTodoTask
        {
            Title = "Task",
            Description = "Do it",
            TodoListId = 10,
            Status = TodoTaskStatus.NotStarted,
        };

        this.todoTaskService
            .Setup(x => x.CreateTaskAsync(
                It.Is<CreateTodoTask>(model =>
                    model.Title == request.Title &&
                    model.UserId == CurrentUserId)))
            .ReturnsAsync(new ModelTodoTask
            {
                Id = 91,
                Title = request.Title,
                Description = request.Description,
                TodoListId = request.TodoListId,
                UserId = CurrentUserId,
            });

        var result = await controller.CreateTask(request);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TodoTaskController.GetTaskById));
        created.RouteValues!["id"].Should().Be(91);
        request.UserId.Should().Be(CurrentUserId);
    }

    [Fact]
    public async Task UpdateTaskWhenTaskDoesNotExistReturnsNotFoundAndDoesNotUpdate()
    {
        var controller = this.CreateController();

        this.todoTaskService
            .Setup(x => x.GetByIdTaskAsync(91, CurrentUserId))
            .ReturnsAsync((ModelTodoTask?)null);

        var result = await controller.UpdateTask(
            91,
            new UpdateTodoTask
            {
                Id = 91,
                Title = "Missing",
                Description = "Missing",
                TodoListId = 10,
            });

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().Be("Task with id 91 not found");
        this.todoTaskService.Verify(
            x => x.UpdateTaskAsync(It.IsAny<int>(), It.IsAny<UpdateTodoTask>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTasksByTitleWithBlankTitleReturnsBadRequestAndDoesNotQuery()
    {
        var controller = this.CreateController();

        var result = await controller.GetTasksByTitle(title: " ");

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Title is required.");
        this.todoTaskService.Verify(
            x => x.GetAllTasksByTitleAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAllTasksPassesAuthenticatedUserAndReturnsPagedResponse()
    {
        var controller = this.CreateController();
        var response = new PagedResponse<ModelTodoTask>
        {
            Items = new[]
            {
                new ModelTodoTask
                {
                    Id = 91,
                    Title = "Task",
                    TodoListId = 10,
                    UserId = CurrentUserId,
                },
            },
            Page = 1,
            PageSize = 8,
            TotalCount = 1,
        };

        this.todoTaskService
            .Setup(x => x.GetAllTasksAsync(1, 8, 10, CurrentUserId, TodoTaskStatus.Completed, "asc(title)"))
            .ReturnsAsync(response);

        var result = await controller.GetAllTasks(1, 8, 10, TodoTaskStatus.Completed, "asc(title)");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(response);
    }

    private TodoTaskController CreateController()
    {
        var controller = new TodoTaskController(this.todoTaskService.Object);
        controller.SignInAs(CurrentUserId);
        return controller;
    }
}
