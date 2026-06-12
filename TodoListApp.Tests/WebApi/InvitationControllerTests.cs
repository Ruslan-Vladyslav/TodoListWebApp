using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoListApp.Services.Interfaces;
using TodoListApp.Tests.TestSupport;
using TodoListApp.WebApi.Controllers;
using TodoListApp.WebApi.Models.Enums;
using TodoListApp.WebApi.Models.Models.Invitation;

namespace TodoListApp.Tests.WebApi;

public class InvitationControllerTests
{
    private const string SenderUserId = "sender-user";
    private const string ReceiverUserId = "receiver-user";

    private readonly Mock<IInvitationService> invitationService = new(MockBehavior.Strict);

    [Fact]
    public async Task SendUsesAuthenticatedSenderAndReturnsOk()
    {
        var controller = this.CreateController();
        var request = new SendInvitationRequest
        {
            SenderId = "spoofed-user",
            ReceiverId = ReceiverUserId,
            ListId = 22,
            Role = TodoListRole.Editor,
            Message = "Join this list",
        };

        this.invitationService
            .Setup(x => x.SendInvitationAsync(
                SenderUserId,
                ReceiverUserId,
                22,
                TodoListRole.Editor,
                request.Message))
            .Returns(Task.CompletedTask);

        var result = await controller.Send(request);

        result.Should().BeOfType<OkResult>();
        this.invitationService.VerifyAll();
    }

    [Fact]
    public async Task HasPendingReturnsBooleanResult()
    {
        var controller = this.CreateController();

        this.invitationService
            .Setup(x => x.HasPendingInvitationAsync(22, ReceiverUserId))
            .ReturnsAsync(true);

        var result = await controller.HasPending(22, ReceiverUserId);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(true);
    }

    [Fact]
    public async Task AcceptInvokesServiceAndReturnsOk()
    {
        var controller = this.CreateController();

        this.invitationService
            .Setup(x => x.AcceptInvitationAsync(31))
            .Returns(Task.CompletedTask);

        var result = await controller.Accept(31);

        result.Should().BeOfType<OkResult>();
        this.invitationService.VerifyAll();
    }

    [Fact]
    public async Task GetUserInvitationsUsesAuthenticatedUser()
    {
        var controller = this.CreateController();
        var invitations = new[]
        {
            new ModelInvitation
            {
                Id = 31,
                TodoListId = 22,
                TodoListTitle = "Shared list",
                SenderUserId = SenderUserId,
                ReceiverUserId = ReceiverUserId,
                Role = TodoListRole.Viewer,
                Status = InvitationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            },
        };

        this.invitationService
            .Setup(x => x.GetUserInvitationsAsync(SenderUserId))
            .ReturnsAsync(invitations);

        var result = await controller.GetUserInvitations();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(invitations);
    }

    private InvitationController CreateController()
    {
        var controller = new InvitationController(this.invitationService.Object);
        controller.SignInAs(SenderUserId);
        return controller;
    }
}
