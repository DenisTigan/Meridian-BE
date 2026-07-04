using System.Security.Claims;
using MeridianEmployeeHub.Services.Notifications;
using MeridianEmployeeHub.Services.Notifications.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeridianEmployeeHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET /api/v1/notifications/me
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications([FromQuery] int skip = 0, [FromQuery] int take = 50)
        {
            var currentUserId = GetCurrentEmployeeId();
            var notifications = await _notificationService.GetMyNotificationsAsync(currentUserId, skip, take);
            return Ok(notifications);
        }

        // GET /api/v1/notifications/me/unread-count
        [HttpGet("me/unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var currentUserId = GetCurrentEmployeeId();
            var count = await _notificationService.GetUnreadCountAsync(currentUserId);
            return Ok(count);
        }

        // PATCH /api/v1/notifications/{id}/read
        [HttpPatch("{id:int}/read")]
        public async Task<ActionResult<NotificationDto>> MarkAsRead(int id)
        {
            var currentUserId = GetCurrentEmployeeId();
            var notification = await _notificationService.MarkAsReadAsync(id, currentUserId);
            return Ok(notification);
        }

        // PATCH /api/v1/notifications/read-all
        [HttpPatch("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var currentUserId = GetCurrentEmployeeId();
            await _notificationService.MarkAllAsReadAsync(currentUserId);
            return NoContent();
        }

        private int GetCurrentEmployeeId()
        {
            var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");

            if (!int.TryParse(subClaim, out var employeeId))
                throw new UnauthorizedAccessException("Invalid token: missing or invalid subject claim.");

            return employeeId;
        }
    }
}
