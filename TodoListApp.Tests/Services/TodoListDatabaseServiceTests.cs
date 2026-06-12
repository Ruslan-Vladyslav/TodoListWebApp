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
using TodoListApp.WebApi.Models.Models.TodoList;

namespace TodoListApp.Tests.Services;

public class TodoListDatabaseServiceTests
{
    private const string OwnerUserId = "owner-user";
    private const string SharedUserId = "shared-user";

    private readonly Mock<INotificationService> notificationService = new(MockBehavior.Strict);
    private readonly Mock<IUserService> userService = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateListWhenModelIsValidPersistsAndReturnsModel()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = this.CreateService(context);

        var result = await service.CreateListAsync(new CreateTodoList
        {
            Title = "New list",
            Description = "Created",
            UserId = OwnerUserId,
        });

        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("New list");
        result.UserId.Should().Be(OwnerUserId);

        var entity = await context.TodoLists.SingleAsync();
        entity.Title.Should().Be("New list");
        entity.UserId.Should().Be(OwnerUserId);
    }

    [Fact]
    public async Task UpdateListWhenListExistsUpdatesTitleAndDescription()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Old",
            Description = "Before",
            UserId = OwnerUserId,
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        await service.UpdateListAsync(
            10,
            new UpdateTodoList
            {
                Id = 10,
                Title = "Updated",
                Description = "After",
            });

        var entity = await context.TodoLists.SingleAsync();
        entity.Title.Should().Be("Updated");
        entity.Description.Should().Be("After");
    }

    [Fact]
    public async Task DeleteListNotifiesAcceptedMembersAndRemovesList()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Shared list",
            UserId = OwnerUserId,
            Accesses = new List<TodoListAccessEntity>
            {
                new()
                {
                    OwnerUserId = OwnerUserId,
                    TargetUserId = SharedUserId,
                    Role = TodoListRole.Viewer,
                    Accepted = true,
                },
                new()
                {
                    OwnerUserId = OwnerUserId,
                    TargetUserId = "pending-user",
                    Role = TodoListRole.Viewer,
                    Accepted = false,
                },
            },
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        this.notificationService
            .Setup(x => x.CreateAsync(It.Is<CreateNotification>(n =>
                n.UserId == SharedUserId &&
                n.Type == NotificationType.ListDeleted &&
                n.TodoListId == 10)))
            .ReturnsAsync(new ModelNotification
            {
                Id = 1,
                UserId = SharedUserId,
                Text = "Deleted",
                Type = NotificationType.ListDeleted,
                TodoListId = 10,
            });

        await service.DeleteListAsync(10);

        context.TodoLists.Should().BeEmpty();
        this.notificationService.VerifyAll();
        this.notificationService.Verify(
            x => x.CreateAsync(It.IsAny<CreateNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllListByUserReturnsOwnedAndAcceptedSharedListsOnly()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.AddRange(
            new TodoListEntity
            {
                Id = 10,
                Title = "Owned",
                UserId = OwnerUserId,
                TodoTasks = new List<TodoTaskEntity>
                {
                    new()
                    {
                        Title = "Task",
                        Description = "Owned task",
                        CreatedByUserId = OwnerUserId,
                        DueDate = DateTime.UtcNow.AddDays(1),
                        Status = TodoTaskStatus.NotStarted,
                    },
                },
            },
            new TodoListEntity
            {
                Id = 11,
                Title = "Shared",
                UserId = "other-owner",
                Accesses = new List<TodoListAccessEntity>
                {
                    new()
                    {
                        OwnerUserId = "other-owner",
                        TargetUserId = OwnerUserId,
                        Role = TodoListRole.Editor,
                        Accepted = true,
                    },
                },
            },
            new TodoListEntity
            {
                Id = 12,
                Title = "Pending",
                UserId = "other-owner",
                Accesses = new List<TodoListAccessEntity>
                {
                    new()
                    {
                        OwnerUserId = "other-owner",
                        TargetUserId = OwnerUserId,
                        Role = TodoListRole.Viewer,
                        Accepted = false,
                    },
                },
            });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        var result = await service.GetAllListByUserAsync(1, 10, OwnerUserId);

        result.TotalCount.Should().Be(2);
        result.Items.Select(x => x.Title).Should().BeEquivalentTo("Owned", "Shared");
        result.Items.Single(x => x.Title == "Owned").TaskCount.Should().Be(1);
    }

    private TodoListDatabaseService CreateService(TodoListApp.Services.Database.TodoListDbContext context)
    {
        return new TodoListDatabaseService(
            context,
            this.notificationService.Object,
            this.userService.Object);
    }
}
