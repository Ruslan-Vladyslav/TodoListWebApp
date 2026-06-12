using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Notification;
using ApiNotificationController = TodoListApp.WebApi.Controllers.NotificationController;

namespace TodoListApp.Tests.WebApi;

public class NotificationControllerTests
{
    private const string CurrentUserId = "current-user";

    private readonly Mock<INotificationService> notificationService = new(MockBehavior.Strict);

    [Fact]
    public async Task GetUserNotificationsUsesAuthenticatedUser()
    {
        var controller = this.CreateController();
        var notifications = new[]
        {
            new ModelNotification
            {
                Id = 1,
                UserId = CurrentUserId,
                Text = "Shared",
                Type = NotificationType.InvitationSent,
            },
        };

        this.notificationService
            .Setup(x => x.GetUserNotificationsAsync(CurrentUserId))
            .ReturnsAsync(notifications);

        var result = await controller.GetUserNotifications();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(notifications);
    }

    [Fact]
    public async Task CreateReturnsCreatedNotification()
    {
        var controller = this.CreateController();
        var request = new CreateNotification
        {
            UserId = CurrentUserId,
            Text = "Done",
            Type = NotificationType.TaskCompleted,
            TodoTaskId = 9,
        };

        var created = new ModelNotification
        {
            Id = 1,
            UserId = CurrentUserId,
            Text = request.Text,
            Type = request.Type,
            TodoTaskId = request.TodoTaskId,
        };

        this.notificationService
            .Setup(x => x.CreateAsync(request))
            .ReturnsAsync(created);

        var result = await controller.Create(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(created);
    }

    [Fact]
    public async Task MarkReadInvokesServiceAndReturnsNoContent()
    {
        var controller = this.CreateController();

        this.notificationService
            .Setup(x => x.MarkAsReadAsync(1))
            .Returns(Task.CompletedTask);

        var result = await controller.MarkRead(1);

        result.Should().BeOfType<NoContentResult>();
        this.notificationService.VerifyAll();
    }

    [Fact]
    public async Task DeleteInvokesServiceAndReturnsNoContent()
    {
        var controller = this.CreateController();

        this.notificationService
            .Setup(x => x.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
        this.notificationService.VerifyAll();
    }

    private ApiNotificationController CreateController()
    {
        var controller = new ApiNotificationController(this.notificationService.Object);
        controller.SignInAs(CurrentUserId);
        return controller;
    }
}
