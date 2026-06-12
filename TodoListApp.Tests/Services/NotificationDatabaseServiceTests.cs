using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Services;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Notification;

namespace TodoListApp.Tests.Services;

public class NotificationDatabaseServiceTests
{
    private const string UserId = "user-id";

    [Fact]
    public async Task CreateAsyncPersistsUnreadNotification()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = new NotificationDatabaseService(context);

        var result = await service.CreateAsync(new CreateNotification
        {
            UserId = UserId,
            Text = "Assigned",
            Type = NotificationType.TaskAssigned,
            TodoTaskId = 30,
        });

        result.Id.Should().BeGreaterThan(0);
        result.IsRead.Should().BeFalse();
        result.Type.Should().Be(NotificationType.TaskAssigned);

        var entity = await context.Notifications.SingleAsync();
        entity.UserId.Should().Be(UserId);
        entity.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserNotificationsReturnsOnlyUserNotificationsNewestFirst()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.Notifications.AddRange(
            new NotificationEntity
            {
                Id = 1,
                UserId = UserId,
                Text = "Older",
                Type = NotificationType.TaskAssigned,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            },
            new NotificationEntity
            {
                Id = 2,
                UserId = UserId,
                Text = "Newer",
                Type = NotificationType.TaskCompleted,
                CreatedAt = DateTime.UtcNow,
            },
            new NotificationEntity
            {
                Id = 3,
                UserId = "other-user",
                Text = "Hidden",
                Type = NotificationType.TaskAssigned,
                CreatedAt = DateTime.UtcNow.AddDays(1),
            });
        await context.SaveChangesAsync();
        var service = new NotificationDatabaseService(context);

        var result = (await service.GetUserNotificationsAsync(UserId)).ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Text).Should().ContainInOrder("Newer", "Older");
    }

    [Fact]
    public async Task MarkAsReadWhenNotificationExistsSetsIsRead()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.Notifications.Add(new NotificationEntity
        {
            Id = 1,
            UserId = UserId,
            Text = "Unread",
            Type = NotificationType.TaskAssigned,
            IsRead = false,
        });
        await context.SaveChangesAsync();
        var service = new NotificationDatabaseService(context);

        await service.MarkAsReadAsync(1);

        var entity = await context.Notifications.SingleAsync();
        entity.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWhenNotificationMissingThrowsKeyNotFound()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = new NotificationDatabaseService(context);

        var act = () => service.DeleteAsync(99);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Notification not found");
    }
}
