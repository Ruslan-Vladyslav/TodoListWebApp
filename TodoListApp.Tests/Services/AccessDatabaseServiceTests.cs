using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Services.Database.Entities;
using TodoListApp.Services.Database.Entity;
using TodoListApp.Services.Database.Services;
using TodoListApp.Services.Enums;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.Tests.Services;

public class AccessDatabaseServiceTests
{
    private const string OwnerUserId = "owner-user";
    private const string TargetUserId = "target-user";
    private const string StrangerUserId = "stranger-user";

    [Fact]
    public async Task GrantAccessWhenCallerIsOwnerCreatesAcceptedAccess()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Owned list",
            UserId = OwnerUserId,
        });
        await context.SaveChangesAsync();

        var service = new AccessDatabaseService(context);

        await service.GrantAccessAsync(OwnerUserId, TargetUserId, 10, TodoListRole.Editor);

        var access = await context.TodoListAccesses.SingleAsync();
        access.TodoListId.Should().Be(10);
        access.OwnerUserId.Should().Be(OwnerUserId);
        access.TargetUserId.Should().Be(TargetUserId);
        access.Role.Should().Be(TodoListRole.Editor);
        access.Accepted.Should().BeTrue();
        access.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GrantAccessWhenCallerIsNotOwnerThrowsUnauthorizedAndDoesNotCreateAccess()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Owned list",
            UserId = OwnerUserId,
        });
        await context.SaveChangesAsync();

        var service = new AccessDatabaseService(context);

        var act = () => service.GrantAccessAsync(StrangerUserId, TargetUserId, 10, TodoListRole.Viewer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        context.TodoListAccesses.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateRoleWhenTargetIsOwnerThrowsInvalidOperation()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Owned list",
            UserId = OwnerUserId,
        });
        await context.SaveChangesAsync();

        var service = new AccessDatabaseService(context);

        var act = () => service.UpdateRoleAsync(OwnerUserId, OwnerUserId, 10, TodoListRole.Viewer);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot change the owner's role.");
    }

    [Fact]
    public async Task RevokeAccessReassignsTasksToOwnerAndRemovesAccess()
    {
        await using var context = DatabaseTestHelpers.CreateContext();
        context.TodoLists.Add(new TodoListEntity
        {
            Id = 10,
            Title = "Owned list",
            UserId = OwnerUserId,
        });
        context.TodoListAccesses.Add(new TodoListAccessEntity
        {
            TodoListId = 10,
            OwnerUserId = OwnerUserId,
            TargetUserId = TargetUserId,
            Role = TodoListRole.Editor,
            Accepted = true,
        });
        context.TodoTasks.Add(new TodoTaskEntity
        {
            Id = 30,
            Title = "Assigned task",
            Description = "Needs reassignment",
            CreatedByUserId = OwnerUserId,
            AssignedToUserId = TargetUserId,
            DueDate = DateTime.UtcNow.AddDays(1),
            TodoListId = 10,
            Status = TodoTaskStatus.NotStarted,
        });
        await context.SaveChangesAsync();

        var service = new AccessDatabaseService(context);

        await service.RevokeAccessAsync(OwnerUserId, TargetUserId, 10);

        context.TodoListAccesses.Should().BeEmpty();
        var task = await context.TodoTasks.SingleAsync();
        task.AssignedToUserId.Should().Be(OwnerUserId);
    }
}
