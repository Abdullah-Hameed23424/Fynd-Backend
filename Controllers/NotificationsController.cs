
using Fynd.Api.Data;
using Fynd.Api.DTOs.Notifications;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fynd.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IFirebaseNotificationService _notificationService;
        private readonly ApplicationDbContext _db;

        public NotificationsController(
            IFirebaseNotificationService notificationService,
            ApplicationDbContext db)
        {
            _notificationService = notificationService;
            _db = db;
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendTestNotification(
            [FromBody] SendNotificationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FcmToken) ||
                string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Body))
            {
                return BadRequest(new
                {
                    message = "FCM token, title and body are required."
                });
            }

            try
            {
                var messageId = await _notificationService.SendAsync(
                    request.FcmToken,
                    request.Title,
                    request.Body,
                    cancellationToken);

                return Ok(new
                {
                    message = "Notification sent successfully.",
                    messageId
                });
            }
            catch (FirebaseMessagingException ex)
            {
                return BadRequest(new
                {
                    message = "Firebase could not send the notification.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice(
            [FromBody] RegisterDeviceTokenRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FcmToken) ||
                string.IsNullOrWhiteSpace(request.InstallationId))
            {
                return BadRequest(new
                {
                    message = "FCM token and installation ID are required."
                });
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("userId")?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("id")?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new
                {
                    message = "The authenticated JWT does not contain a valid user ID."
                });
            }

            var userExists = await _db.Users
                .AnyAsync(x => x.Id == userId, cancellationToken);

            if (!userExists)
            {
                return Unauthorized(new
                {
                    message = "The authenticated user was not found."
                });
            }

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(request.FcmToken)));

            var byInstallation = await _db.UserDeviceTokens
                .SingleOrDefaultAsync(
                    x => x.InstallationId == request.InstallationId,
                    cancellationToken);

            var byToken = await _db.UserDeviceTokens
                .SingleOrDefaultAsync(
                    x => x.TokenHash == tokenHash,
                    cancellationToken);

            UserDeviceToken device;

            if (byInstallation != null &&
                byToken != null &&
                byInstallation.Id != byToken.Id)
            {
                _db.UserDeviceTokens.Remove(byInstallation);
                device = byToken;
            }
            else
            {
                device = byInstallation ?? byToken
                    ?? new UserDeviceToken();

                if (device.Id == 0)
                {
                    _db.UserDeviceTokens.Add(device);
                }
            }

            device.UserId = userId;
            device.InstallationId = request.InstallationId;
            device.FcmToken = request.FcmToken;
            device.TokenHash = tokenHash;
            device.IsActive = true;
            device.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Device token registered successfully.",
                deviceId = device.Id
            });
        }
    }
}
