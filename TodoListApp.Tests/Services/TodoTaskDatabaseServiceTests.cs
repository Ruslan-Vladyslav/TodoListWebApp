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
using TodoListApp.WebApi.Models.Models.TodoTask;

namespace TodoListApp.Tests.Services;

public class TodoTaskDatabaseServiceTests
{
    private const string OwnerUserId = "owner-user";
    private const string EditorUserId = "editor-user";
    private const string AssignedUserId = "assigned-user";

    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);
    private readonly Mock<INotificationService> notificationService = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateTaskWhenUserIsViewerThrowsUnauthorizedAndDoesNotCreateTask()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(EditorUserId, 10))
            .ReturnsAsync(TodoListRole.Viewer);

        var act = () => service.CreateTaskAsync(new CreateTodoTask
        {
            Title = "Blocked task",
            Description = "Viewer cannot create",
            UserId = EditorUserId,
            TodoListId = 10,
            DueDate = DateTime.UtcNow.AddDays(1),
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        context.TodoTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTaskWhenAssignedToAnotherUserCreatesTaskAndSendsNotification()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(EditorUserId, 10))
            .ReturnsAsync(TodoListRole.Editor);

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == AssignedUserId &&
                n.Type == NotificationType.TaskAssigned)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 1,
                UserId = AssignedUserId,
                Text = "Assigned",
                Type = NotificationType.TaskAssigned,
            });

        var result = await service.CreateTaskAsync(new CreateTodoTask
        {
            Title = "Created task",
            Description = "Editor can create",
            UserId = EditorUserId,
            AssignedUserId = AssignedUserId,
            TodoListId = 10,
            Status = TodoTaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(1),
        });

        result.Id.Should().BeGreaterThan(0);
        result.AssignedUserId.Should().Be(AssignedUserId);
        var task = await context.TodoTasks.SingleAsync();
        task.CreatedByUserId.Should().Be(EditorUserId);
        task.AssignedByUserId.Should().Be(EditorUserId);
        this.notificationService.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskWhenEditorReassignsTaskThrowsUnauthorized()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedTaskAsync(context);
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(EditorUserId, 10))
            .ReturnsAsync(TodoListRole.Editor);

        var act = () => service.UpdateTaskAsync(
            30,
            new UpdateTodoTask
            {
                Id = 30,
                Title = "Updated",
                Description = "Cannot reassign",
                TodoListId = 10,
                AssignedUserId = AssignedUserId,
                Status = TodoTaskStatus.InProgress,
                DueDate = DateTime.UtcNow.AddDays(2),
            },
            EditorUserId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Only owner can reassign task");
    }

    [Fact]
    public async Task DeleteTaskWhenUserIsNotOwnerThrowsUnauthorizedAndKeepsTask()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedTaskAsync(context);
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(EditorUserId, 10))
            .ReturnsAsync(TodoListRole.Editor);

        var act = () => service.DeleteTaskAsync(30, EditorUserId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        (await context.TodoTasks.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UpdateTaskWhenEditorCompletesTaskNotifiesOwner()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedTaskAsync(context);
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(EditorUserId, 10))
            .ReturnsAsync(TodoListRole.Editor);

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == OwnerUserId &&
                n.Type == NotificationType.TaskCompleted)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 2,
                UserId = OwnerUserId,
                Text = "Completed",
                Type = NotificationType.TaskCompleted,
            });

        await service.UpdateTaskAsync(
            30,
            new UpdateTodoTask
            {
                Id = 30,
                Title = "Completed task",
                Description = "Done",
                TodoListId = 10,
                Status = TodoTaskStatus.Completed,
                DueDate = DateTime.UtcNow.AddDays(2),
            },
            EditorUserId);

        var task = await context.TodoTasks.SingleAsync();
        task.Status.Should().Be(TodoTaskStatus.Completed);
        this.notificationService.VerifyAll();
    }

    private TodoTaskDatabaseService CreateService(TodoListApp.Services.Database.TodoListDbContext context)
    {
        return new TodoTaskDatabaseService(
            context,
            this.accessService.Object,
            this.notificationService.Object);
    }

    private async Task SeedTaskAsync(TodoListApp.Services.Database.TodoListDbContext context)
    {
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Owned list",
            UserId = OwnerUserId,
        });
        context.TodoTasks.Add(new TodoTaskEntity
        {
            Id = 30,
            Title = "Original task",
            Description = "Before update",
            CreatedByUserId = OwnerUserId,
            TodoListId = 10,
            Status = TodoTaskStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(1),
        });
        await context.SaveChangesAsync();
    }
}
