
using System.Security.Claims;
using Fynd.Api.DTOs.Notifications;
using Fynd.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("id")?.Value;

            return int.TryParse(userIdClaim, out var userId)
                ? userId
                : null;
        }

        // GET: api/notifications?page=1&pageSize=20
        [HttpGet]
        public async Task<ActionResult<NotificationListResponse>>
            GetNotifications(
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 20,
                CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var result =
                await _notificationService.GetNotificationsAsync(
                    userId.Value,
                    page,
                    pageSize,
                    cancellationToken);

            return Ok(result);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var count =
                await _notificationService.GetUnreadCountAsync(
                    userId.Value,
                    cancellationToken);

            return Ok(new { unreadCount = count });
        }

        // PATCH: api/notifications/{id}/read
        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var success = await _notificationService.MarkAsReadAsync(
                userId.Value,
                id,
                cancellationToken);

            if (!success)
            {
                return NotFound(new
                {
                    message = "Notification not found."
                });
            }

            return Ok(new
            {
                message = "Notification marked as read.",
                id,
                isRead = true
            });
        }

        // PATCH: api/notifications/read-all
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var updatedCount =
                await _notificationService.MarkAllAsReadAsync(
                    userId.Value,
                    cancellationToken);

            return Ok(new
            {
                message = "All notifications marked as read.",
                updatedCount,
                unreadCount = 0
            });
        }

        // POST: api/notifications/test
        [HttpPost("test")]
        public async Task<IActionResult> SendTestNotification(
            [FromBody] SendNotificationRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var messageId = await _notificationService.SendTestAsync(
                request.FcmToken,
                request.Title,
                request.Body,
                cancellationToken);

            return Ok(new
            {
                message = "Test notification sent.",
                messageId
            });
        }

        // POST: api/notifications/register-device
        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice(
            [FromBody] RegisterDeviceTokenRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var deviceId = await _notificationService.RegisterDeviceAsync(
                userId.Value,
                request.FcmToken,
                request.InstallationId,
                cancellationToken);

            if (deviceId == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(new
            {
                message = "Device registered successfully.",
                deviceId
            });
        }
    }
}
