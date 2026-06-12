using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Models.Common;
using TodoListApp.WebApi.Models.Models.TodoList;
using TodoListApp.WebApp.Controllers;

namespace TodoListApp.Tests.Controllers;

public class TodoListControllerTests
{
    private const string CurrentUserId = "current-user";
    private const string OtherUserId = "other-user";

    private readonly Mock<ITodoListService> todoListService = new(MockBehavior.Strict);
    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);

    [Fact]
    public async Task IndexLoadsListsForAuthenticatedUser()
    {
        var controller = this.CreateController();
        var response = new PagedResponse<ModelTodoList>
        {
            Items = new[]
            {
                new ModelTodoList
                {
                    Id = 1,
                    Title = "Mine",
                    UserId = CurrentUserId,
                },
            },
            Page = 2,
            PageSize = 8,
            TotalCount = 9,
        };

        this.todoListService
            .Setup(x => x.GetAllListByUserAsync(2, 8, CurrentUserId))
            .ReturnsAsync(response);

        var result = await controller.Index(2);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeAssignableTo<List<ModelTodoList>>().Subject;
        model.Should().ContainSingle(x => x.Id == 1);
        ((string)controller.ViewBag.CurrentUserId).Should().Be(CurrentUserId);
        ((int)controller.ViewBag.CurrentPage).Should().Be(2);
        ((int)controller.ViewBag.TotalPages).Should().Be(2);
    }

    [Fact]
    public async Task CreatePostAssignsAuthenticatedUserAndRedirectsToIndex()
    {
        var controller = this.CreateController();
        var model = new CreateTodoList
        {
            Title = "New list",
            Description = "From MVC",
        };

        this.todoListService
            .Setup(x => x.CreateListAsync(
                It.Is<CreateTodoList>(item =>
                    item.Title == model.Title &&
                    item.UserId == CurrentUserId)))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 5,
                Title = model.Title,
                UserId = CurrentUserId,
            });

        var result = await controller.Create(model);

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(TodoListController.Index));
        model.UserId.Should().Be(CurrentUserId);
    }

    [Fact]
    public async Task EditGetWhenCurrentUserIsNotOwnerReturnsForbid()
    {
        var controller = this.CreateController();

        this.todoListService
            .Setup(x => x.GetByIdListAsync(5))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 5,
                Title = "Other list",
                UserId = OtherUserId,
            });

        var result = await controller.Edit(5);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteWhenCurrentUserIsNotOwnerReturnsForbidAndDoesNotDelete()
    {
        var controller = this.CreateController();

        this.todoListService
            .Setup(x => x.GetByIdListAsync(5))
            .ReturnsAsync(new ModelTodoList
            {
                Id = 5,
                Title = "Other list",
                UserId = OtherUserId,
            });

        var result = await controller.Delete(5);

        result.Should().BeOfType<ForbidResult>();
        this.todoListService.Verify(x => x.DeleteListAsync(It.IsAny<int>()), Times.Never);
    }

    private TodoListController CreateController()
    {
        var controller = new TodoListController(
            this.todoListService.Object,
            this.accessService.Object);

        controller.SignInAs(CurrentUserId);
        return controller;
    }
}
