using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoListApp.Services.Database.Entity;
using TodoListApp.Services.Database.Services;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Notification;

namespace TodoListApp.Tests.Services;

public class InvitationDatabaseServiceTests
{
    private const string OwnerUserId = "owner-user";
    private const string ReceiverUserId = "receiver-user";
    private const string StrangerUserId = "stranger-user";

    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);
    private readonly Mock<INotificationService> notificationService = new(MockBehavior.Strict);
    private readonly Mock<IUserService> userService = new(MockBehavior.Strict);

    [Fact]
    public async Task SendInvitationWhenSenderInvitesSelfThrowsInvalidOperation()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = this.CreateService(context);

        var act = () => service.SendInvitationAsync(
            OwnerUserId,
            OwnerUserId,
            10,
            TodoListRole.Viewer);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot invite yourself");
    }

    [Fact]
    public async Task SendInvitationWhenSenderIsNotOwnerThrowsUnauthorized()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedListAsync(context);
        var service = this.CreateService(context);

        var act = () => service.SendInvitationAsync(
            StrangerUserId,
            ReceiverUserId,
            10,
            TodoListRole.Viewer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only owner can share list");
    }

    [Fact]
    public async Task SendInvitationWhenPendingInvitationExistsThrowsInvalidOperation()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedListAsync(context);
        context.TodoInvitations.Add(new TodoInvitationEntity
        {
            TodoListId = 10,
            SenderUserId = OwnerUserId,
            ReceiverUserId = ReceiverUserId,
            Role = TodoListRole.Viewer,
            Status = InvitationStatus.Pending,
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        var act = () => service.SendInvitationAsync(
            OwnerUserId,
            ReceiverUserId,
            10,
            TodoListRole.Editor);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Pending invitation already exists");
    }

    [Fact]
    public async Task SendInvitationWhenValidCreatesPendingInvitationAndNotification()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedListAsync(context);
        var service = this.CreateService(context);

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == ReceiverUserId &&
                n.Type == NotificationType.InvitationSent &&
                n.TodoListId == 10)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 1,
                UserId = ReceiverUserId,
                Text = "Invitation",
                Type = NotificationType.InvitationSent,
                TodoListId = 10,
            });

        await service.SendInvitationAsync(
            OwnerUserId,
            ReceiverUserId,
            10,
            TodoListRole.Editor,
            "Please join");

        var invitation = await context.TodoInvitations.SingleAsync();
        invitation.SenderUserId.Should().Be(OwnerUserId);
        invitation.ReceiverUserId.Should().Be(ReceiverUserId);
        invitation.Role.Should().Be(TodoListRole.Editor);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.Message.Should().Be("Please join");
        this.notificationService.VerifyAll();
    }

    [Fact]
    public async Task AcceptInvitationMarksAcceptedGrantsAccessAndNotifiesSender()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedListAsync(context);
        context.TodoInvitations.Add(new TodoInvitationEntity
        {
            Id = 50,
            TodoListId = 10,
            SenderUserId = OwnerUserId,
            ReceiverUserId = ReceiverUserId,
            Role = TodoListRole.Editor,
            Status = InvitationStatus.Pending,
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GrantAccessAsync(OwnerUserId, ReceiverUserId, 10, TodoListRole.Editor))
            .Returns(Task.CompletedTask);

        this.userService
            .Setup(x => x.GetUserNameAsync(ReceiverUserId))
            .ReturnsAsync("Receiver");

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == OwnerUserId &&
                n.Type == NotificationType.InvitationAccepted &&
                n.TodoListId == 10)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 2,
                UserId = OwnerUserId,
                Text = "Accepted",
                Type = NotificationType.InvitationAccepted,
                TodoListId = 10,
            });

        await service.AcceptInvitationAsync(50);

        var invitation = await context.TodoInvitations.SingleAsync();
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.RespondedAt.Should().NotBeNull();
        this.accessService.VerifyAll();
        this.notificationService.VerifyAll();
    }

    [Fact]
    public async Task RejectInvitationMarksRejectedAndNotifiesSender()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedListAsync(context);
        context.TodoInvitations.Add(new TodoInvitationEntity
        {
            Id = 50,
            TodoListId = 10,
            SenderUserId = OwnerUserId,
            ReceiverUserId = ReceiverUserId,
            Role = TodoListRole.Viewer,
            Status = InvitationStatus.Pending,
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        this.userService
            .Setup(x => x.GetUserNameAsync(ReceiverUserId))
            .ReturnsAsync("Receiver");

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == OwnerUserId &&
                n.Type == NotificationType.InvitationRejected &&
                n.TodoListId == 10)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 3,
                UserId = OwnerUserId,
                Text = "Rejected",
                Type = NotificationType.InvitationRejected,
                TodoListId = 10,
            });

        await service.RejectInvitationAsync(50);

        var invitation = await context.TodoInvitations.SingleAsync();
        invitation.Status.Should().Be(InvitationStatus.Rejected);
        invitation.RespondedAt.Should().NotBeNull();
        this.notificationService.VerifyAll();
    }

    private InvitationDatabaseService CreateService(TodoListApp.Services.Database.TodoListDbContext context)
    {
        return new InvitationDatabaseService(
            context,
            this.accessService.Object,
            this.notificationService.Object,
            this.userService.Object);
    }

    private async Task SeedListAsync(TodoListApp.Services.Database.TodoListDbContext context)
    {
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Owned list",
            UserId = OwnerUserId,
        });
        await context.SaveChangesAsync();
    }
}
