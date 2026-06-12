using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Controllers;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoTag;
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Tests.WebApi;

public class TodoTagControllerTests
{
    private const string CurrentUserId = "current-user";

    private readonly Mock<ITodoTagService> todoTagService = new(MockBehavior.Strict);

    [Fact]
    public async Task GetTagByIdWhenMissingReturnsNotFound()
    {
        var controller = this.CreateController();

        this.todoTagService
            .Setup(x => x.GetByIdTagAsync(12))
            .ReturnsAsync((ModelTodoTag?)null);

        var result = await controller.GetTagById(12);

        var notFound = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().Be("Tag with id 12 not found");
    }

    [Fact]
    public async Task CreateTagWithInvalidModelReturnsBadRequestAndDoesNotCreate()
    {
        var controller = this.CreateController();
        controller.ModelState.AddModelError(nameof(CreateTagRequest.Name), "Name is required");

        var result = await controller.CreateTag(new CreateTagRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
        this.todoTagService.Verify(x => x.CreateTagAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task AddTagToTaskUsesAuthenticatedUser()
    {
        var controller = this.CreateController();

        this.todoTagService
            .Setup(x => x.AddTagToTaskAsync(7, 12, CurrentUserId))
            .Returns(Task.CompletedTask);

        var result = await controller.AddTagToTask(7, 12);

        result.Should().BeOfType<NoContentResult>();
        this.todoTagService.VerifyAll();
    }

    [Fact]
    public async Task GetTasksByTagUsesAuthenticatedUserAndReturnsTasks()
    {
        var controller = this.CreateController();
        var tasks = new[]
        {
            new ModelTodoTask
            {
                Id = 7,
                Title = "Tagged task",
            },
        };

        this.todoTagService
            .Setup(x => x.GetTasksByTagAsync(12, CurrentUserId))
            .ReturnsAsync(tasks);

        var result = await controller.GetTasksByTag(12);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(tasks);
    }

    [Fact]
    public async Task GetAllTagsReturnsPagedResponse()
    {
        var controller = this.CreateController();
        var response = new PagedResponse<ModelTodoTag>
        {
            Items = new[] { new ModelTodoTag { Id = 12, Name = "Home" } },
            Page = 1,
            PageSize = 10,
            TotalCount = 1,
        };

        this.todoTagService
            .Setup(x => x.GetAllTagsAsync(1, 10))
            .ReturnsAsync(response);

        var result = await controller.GetAllTags();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(response);
    }

    private TodoTagController CreateController()
    {
        var controller = new TodoTagController(this.todoTagService.Object);
        controller.SignInAs(CurrentUserId);
        return controller;
    }
}
