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

namespace TodoListApp.Tests.Services;

public class TodoTagDatabaseServiceTests
{
    private const string UserId = "user-id";

    private readonly Mock<IAccessService> accessService = new(MockBehavior.Strict);

    [Fact]
    public async Task CreateTagTrimsNameAndReusesExistingTagCaseInsensitively()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoTags.Add(new TodoTagEntity
        {
            Id = 5,
            Name = "home",
        });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        var result = await service.CreateTagAsync("  HOME  ");

        result.Id.Should().Be(5);
        result.Name.Should().Be("home");
        (await context.TodoTags.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateTagWhenNameIsBlankThrowsArgumentException()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var service = this.CreateService(context);

        var act = () => service.CreateTagAsync(" ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Tag name cannot be empty.");
    }

    [Fact]
    public async Task AddTagToTaskWhenUserIsViewerThrowsUnauthorizedAndDoesNotAddTag()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedTaskAndTagAsync(context);
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(UserId, 10))
            .ReturnsAsync(TodoListRole.Viewer);

        var act = () => service.AddTagToTaskAsync(30, 5, UserId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        var task = await context.TodoTasks.Include(x => x.Tags).SingleAsync();
        task.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task AddTagToTaskWhenUserCanEditAddsTagOnlyOnce()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        await this.SeedTaskAndTagAsync(context);
        var service = this.CreateService(context);

        this.accessService
            .Setup(x => x.GetUserRoleAsync(UserId, 10))
            .ReturnsAsync(TodoListRole.Editor);

        await service.AddTagToTaskAsync(30, 5, UserId);
        await service.AddTagToTaskAsync(30, 5, UserId);

        var task = await context.TodoTasks.Include(x => x.Tags).SingleAsync();
        task.Tags.Should().ContainSingle(x => x.Id == 5);
    }

    [Fact]
    public async Task GetTasksByTagReturnsOnlyTasksAccessibleByUser()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        var tag = new TodoTagEntity
        {
            Id = 5,
            Name = "home",
        };

        context.TodoLists.AddRange(
            new TodoListEntity
            {
                Id = 10,
                Title = "Owned",
                UserId = UserId,
            },
            new TodoListEntity
            {
                Id = 11,
                Title = "Hidden",
                UserId = "other-user",
            });
        context.TodoTags.Add(tag);
        context.TodoTasks.AddRange(
            new TodoTaskEntity
            {
                Id = 30,
                Title = "Visible",
                Description = "Can see",
                CreatedByUserId = UserId,
                DueDate = DateTime.UtcNow.AddDays(1),
                TodoListId = 10,
                Status = TodoTaskStatus.NotStarted,
                Tags = new List<TodoTagEntity> { tag },
            },
            new TodoTaskEntity
            {
                Id = 31,
                Title = "Hidden",
                Description = "Cannot see",
                CreatedByUserId = "other-user",
                DueDate = DateTime.UtcNow.AddDays(1),
                TodoListId = 11,
                Status = TodoTaskStatus.NotStarted,
                Tags = new List<TodoTagEntity> { tag },
            });
        await context.SaveChangesAsync();
        var service = this.CreateService(context);

        var result = await service.GetTasksByTagAsync(5, UserId);

        result.Should().ContainSingle();
        result.Single().Title.Should().Be("Visible");
    }

    private TodoTagDatabaseService CreateService(TodoListApp.Services.Database.TodoListDbContext context)
    {
        return new TodoTagDatabaseService(context, this.accessService.Object);
    }

    private async Task SeedTaskAndTagAsync(TodoListApp.Services.Database.TodoListDbContext context)
    {
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "List",
            UserId = UserId,
        });
        context.TodoTasks.Add(new TodoTaskEntity
        {
            Id = 30,
            Title = "Task",
            Description = "Tagged later",
            CreatedByUserId = UserId,
            DueDate = DateTime.UtcNow.AddDays(1),
            TodoListId = 10,
            Status = TodoTaskStatus.NotStarted,
        });
        context.TodoTags.Add(new TodoTagEntity
        {
            Id = 5,
            Name = "home",
        });
        await context.SaveChangesAsync();
    }
}
