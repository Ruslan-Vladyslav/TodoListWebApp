using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Controllers;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Tests.WebApi;

public class TodoListControllerTests
{
    private const string CurrentUserId = "current-user";
    private const string OtherUserId = "other-user";

    private readonly Mock<ITodoListService> todoListService = new(MockBehavior.Strict);

    [Fact]
    public async Task GetAllListsByUserWithoutAuthenticatedUserReturnsUnauthorized()
    {
        var controller = new TodoListController(this.todoListService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()),
                },
            },
        };

        var result = await controller.GetAllListsByUser();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateListAssignsAuthenticatedUserAndReturnsCreatedAtAction()
    {
        var controller = this.CreateController();
        var request = new CreateTodoList
        {
            Title = "New list",
            Description = "Created from API",
        };

        this.todoListService
            .Setup(x => x.CreateListAsync(
                It.Is<CreateTodoList>(model =>
                    model.Title == request.Title &&
                    model.UserId == CurrentUserId)))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 44,
                Title = request.Title,
                UserId = CurrentUserId,
            });

        var result = await controller.CreateList(request);

        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TodoListController.GetListById));
        created.RouteValues!["id"].Should().Be(44);
        request.UserId.Should().Be(CurrentUserId);
    }

    [Fact]
    public async Task UpdateListWhenCurrentUserIsNotOwnerReturnsForbidAndDoesNotUpdate()
    {
        var controller = this.CreateController();

        this.todoListService
            .Setup(x => x.GetByIdListAsync(44))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 44,
                Title = "Someone else's list",
                UserId = OtherUserId,
            });

        var result = await controller.UpdateList(
            44,
            new UpdateTodoList
            {
                Id = 44,
                Title = "Changed",
            });

        result.Should().BeOfType<ForbidResult>();
        this.todoListService.Verify(
            x => x.UpdateListAsync(It.IsAny<int>(), It.IsAny<UpdateTodoList>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteListWhenCurrentUserIsOwnerDeletesAndReturnsNoContent()
    {
        var controller = this.CreateController();

        this.todoListService
            .Setup(x => x.GetByIdListAsync(44))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 44,
                Title = "My list",
                UserId = CurrentUserId,
            });

        this.todoListService
            .Setup(x => x.DeleteListAsync(44))
            .Returns(Task.CompletedTask);

        var result = await controller.DeleteList(44);

        result.Should().BeOfType<NoContentResult>();
        this.todoListService.VerifyAll();
    }

    [Fact]
    public async Task GetAllListsReturnsPagedResponse()
    {
        var controller = this.CreateController();
        var response = new PagedResponse<ModelTodoList>
        {
            Items = new[]
            {
                new ModelTodoList
                {
                    Id = 1,
                    Title = "List",
                    UserId = CurrentUserId,
                },
            },
            Page = 2,
            PageSize = 8,
            TotalCount = 9,
        };

        this.todoListService
            .Setup(x => x.GetAllListAsync(2, 8))
            .ReturnsAsync(response);

        var result = await controller.GetAllLists(2, 8);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(response);
    }

    private TodoListController CreateController()
    {
        var controller = new TodoListController(this.todoListService.Object);
        controller.SignInAs(CurrentUserId);
        return controller;
    }
}
