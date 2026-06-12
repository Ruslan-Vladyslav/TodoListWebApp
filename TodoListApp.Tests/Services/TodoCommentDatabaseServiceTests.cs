using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Database.Entity;
using TodoListApp.Services.Database.Services;
using TodoListApp.Services.Enums;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Notification;
using TodoListApp.WebApi.Models.Models.TodoComment;

namespace TodoListApp.Tests.Services;

public class TodoCommentDatabaseServiceTests
{
    private const string OwnerUserId = "owner-user";
    private const string AssignedUserId = "assigned-user";
    private const string CommentUserId = "comment-user";

    private readonly Mock<INotificationService> notificationService = new(MockBehavior.Strict);
    private readonly Mock<IUserService> userService = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateCommentWhenTaskMissingThrowsKeyNotFound()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = this.CreateService(context);

        var act = () => service.CreateCommentAsync(
            99,
            new CreateTodoComment
            {
                Text = "Missing",
                UserId = CommentUserId,
            });

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("TodoTask 99 not found");
    }

    [Fact]
    public async Task CreateCommentPersistsCommentAndNotifiesOwnerAndAssignee()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedTaskAsync(context);
        var service = this.CreateService(context);

        this.userService
            .Setup(x => x.GetUserNameAsync(CommentUserId))
            .ReturnsAsync("Commenter");

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == OwnerUserId &&
                n.Type == NotificationType.CommentAdded &&
                n.TodoTaskId == 30)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 1,
                UserId = OwnerUserId,
                Text = "Comment",
                Type = NotificationType.CommentAdded,
                TodoTaskId = 30,
            });

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == AssignedUserId &&
                n.Type == NotificationType.CommentAdded &&
                n.TodoTaskId == 30)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 2,
                UserId = AssignedUserId,
                Text = "Comment",
                Type = NotificationType.CommentAdded,
                TodoTaskId = 30,
            });

        var result = await service.CreateCommentAsync(
            30,
            new CreateTodoComment
            {
                Text = "Looks good",
                UserId = CommentUserId,
            });

        result.Id.Should().BeGreaterThan(0);
        result.UserId.Should().Be(CommentUserId);
        (await context.TodoComments.CountAsync()).Should().Be(1);
        this.notificationService.VerifyAll();
    }

    [Fact]
    public async Task DeleteCommentWhenExistsRemovesComment()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoComments.Add(new TodoCommentEntity
        {
            Id = 4,
            Text = "Delete me",
            UserId = CommentUserId,
            TodoTaskId = 30,
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        await service.DeleteCommentAsync(4);

        context.TodoComments.Should().BeEmpty();
    }

    private TodoCommentDatabaseService CreateService(TodoListApp.Services.Database.TodoListDbContext context)
    {
        return new TodoCommentDatabaseService(
            context,
            this.notificationService.Object,
            this.userService.Object);
    }

    private async Task SeedTaskAsync(TodoListApp.Services.Database.TodoListDbContext context)
    {
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "List",
            UserId = OwnerUserId,
        });
        context.TodoTasks.Add(new TodoTaskEntity
        {
            Id = 30,
            Title = "Task",
            Description = "Commented",
            CreatedByUserId = OwnerUserId,
            AssignedToUserId = AssignedUserId,
            DueDate = DateTime.UtcNow.AddDays(1),
            TodoListId = 10,
            Status = TodoTaskStatus.InProgress,
        });
        await context.SaveChangesAsync();
    }
}
