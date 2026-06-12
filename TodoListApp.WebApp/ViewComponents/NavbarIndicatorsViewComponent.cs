using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TodoListApp.Services.Interfaces;
using TodoListApp.WebApi.Models.Enums;

namespace TodoListApp.WebApp.ViewComponents
{
    public class NavbarIndicatorsViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly IInvitationService _invitationService;

        public NavbarIndicatorsViewComponent(INotificationService notificationService, IInvitationService invitationService)
        {
            _notificationService = notificationService;
            _invitationService = invitationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return View(new NavbarIndicatorsViewModel { UnreadNotifications = 0, PendingInvitations = 0 });
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            var unreadNotifications = notifications?.Count(n => !n.IsRead) ?? 0;

            var invitations = await _invitationService.GetUserInvitationsAsync(userId);
            var pendingInvitations = invitations?.Count(i => i.Status == InvitationStatus.Pending) ?? 0;

            var model = new NavbarIndicatorsViewModel
            {
                UnreadNotifications = unreadNotifications,
                PendingInvitations = pendingInvitations
            };

            return View(model);
        }
    }

    public class NavbarIndicatorsViewModel
    {
        public int UnreadNotifications { get; set; }
        public int PendingInvitations { get; set; }
    }
}
