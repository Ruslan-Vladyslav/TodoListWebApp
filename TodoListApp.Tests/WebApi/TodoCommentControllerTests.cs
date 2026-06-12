using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Controllers;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Tests.WebApi;

public class TodoCommentControllerTests
{
    private const string CurrentUserId = "current-user";

    private readonly Mock<ITodoCommentService> todoCommentService = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateCommentAssignsAuthenticatedUserAndReturnsCreatedAtAction()
    {
        var controller = this.CreateController();
        var request = new CreateTodoComment
        {
            Text = "Looks good",
        };

        this.todoCommentService
            .Setup(x => x.CreateCommentAsync(
                9,
                It.Is<CreateTodoComment>(model =>
                    model.Text == request.Text &&
                    model.UserId == CurrentUserId)))
            .ReturnsAsync(new ModelTodoComment
            {
                Id = 4,
                Text = request.Text,
                UserId = CurrentUserId,
            });

        var result = await controller.CreateComment(9, request);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TodoCommentController.GetCommentById));
        created.RouteValues!["id"].Should().Be(4);
        request.UserId.Should().Be(CurrentUserId);
    }

    [Fact]
    public async Task CreateCommentWithInvalidModelReturnsBadRequest()
    {
        var controller = this.CreateController();
        controller.ModelState.AddModelError(nameof(CreateTodoComment.Text), "Text is required");

        var result = await controller.CreateComment(9, new CreateTodoComment());

        result.Should().BeOfType<BadRequestObjectResult>();
        this.todoCommentService.Verify(
            x => x.CreateCommentAsync(It.IsAny<int>(), It.IsAny<CreateTodoComment>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteCommentInvokesServiceAndReturnsNoContent()
    {
        var controller = this.CreateController();

        this.todoCommentService
            .Setup(x => x.DeleteCommentAsync(4))
            .Returns(Task.CompletedTask);

        var result = await controller.DeleteComment(4);

        result.Should().BeOfType<NoContentResult>();
        this.todoCommentService.VerifyAll();
    }

    private TodoCommentController CreateController()
    {
        var controller = new TodoCommentController(this.todoCommentService.Object);
        controller.SignInAs(CurrentUserId);
        return controller;
    }
}
