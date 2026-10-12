
using System.Security.Cryptography;
using System.Text;
using FirebaseAdmin.Messaging;
using Fynd.Api.Data;
using Fynd.Api.DTOs.Notifications;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Create notification in database, then send Firebase push.
        public async Task<int> CreateAndSendAsync(
            int userId,
            string title,
            string message,
            string type = "General",
            int? relatedItemId = null,
            string? relatedItemType = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message is required.", nameof(message));

            var userExists = await _db.Users
                .AnyAsync(x => x.Id == userId, cancellationToken);

            if (!userExists)
                throw new ArgumentException("User not found.", nameof(userId));

            var notification = new AppNotification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedItemId = relatedItemId,
                RelatedItemType = relatedItemType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);

            // Save first, so the notification remains even if push fails.
            await _db.SaveChangesAsync(cancellationToken);

            var devices = await _db.UserDeviceTokens
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var device in devices)
            {
                try
                {
                    await SendFirebaseAsync(
                        device.FcmToken,
                        title,
                        message,
                        cancellationToken);
                }
                catch (FirebaseMessagingException)
                {
                    // The database notification is already saved.
                    // Add logging and invalid-token cleanup as a later improvement.
                }
            }

            return notification.Id;
        }

        // Get notifications with pagination.
        public async Task<NotificationListResponse> GetNotificationsAsync(
            int userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var query = _db.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            var totalCount = await query.CountAsync(cancellationToken);

            var unreadCount = await query.CountAsync(
                x => !x.IsRead,
                cancellationToken);

            var notifications = await query
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new NotificationResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    RelatedItemId = x.RelatedItemId,
                    RelatedItemType = x.RelatedItemType,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt,
                    ReadAt = x.ReadAt
                })
                .ToListAsync(cancellationToken);

            return new NotificationListResponse
            {
                Notifications = notifications,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)pageSize)
            };
        }

        // Get unread notification count.
        public async Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Notifications.CountAsync(
                x => x.UserId == userId && !x.IsRead,
                cancellationToken);
        }

        // Mark one notification as read.
        public async Task<bool> MarkAsReadAsync(
            int userId,
            int notificationId,
            CancellationToken cancellationToken = default)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(
                    x => x.Id == notificationId &&
                         x.UserId == userId,
                    cancellationToken);

            if (notification == null)
                return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

        // Mark all notifications as read.
        public async Task<int> MarkAllAsReadAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _db.Notifications
                .Where(x => x.UserId == userId && !x.IsRead)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(x => x.IsRead, true)
                        .SetProperty(x => x.ReadAt, now),
                    cancellationToken);
        }

        // Send a test push directly through Firebase.
        public async Task<string> SendTestAsync(
            string fcmToken,
            string title,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
                throw new ArgumentException("FCM token is required.", nameof(fcmToken));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title is required.", nameof(title));

            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Body is required.", nameof(body));

            return await SendFirebaseAsync(
                fcmToken,
                title,
                body,
                cancellationToken);
        }

        // Register or update a device token.
        public async Task<int?> RegisterDeviceAsync(
            int userId,
            string fcmToken,
            string installationId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
                throw new ArgumentException("FCM token is required.", nameof(fcmToken));

            if (string.IsNullOrWhiteSpace(installationId))
                throw new ArgumentException("Installation ID is required.", nameof(installationId));

            if (installationId.Length > 100)
                throw new ArgumentException(
                    "Installation ID cannot exceed 100 characters.",
                    nameof(installationId));

            var userExists = await _db.Users
                .AnyAsync(x => x.Id == userId, cancellationToken);

            if (!userExists)
                return null;

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(fcmToken)));

            var device = await _db.UserDeviceTokens
                .FirstOrDefaultAsync(
                    x => x.InstallationId == installationId,
                    cancellationToken);

            if (device == null)
            {
                device = await _db.UserDeviceTokens
                    .FirstOrDefaultAsync(
                        x => x.TokenHash == tokenHash,
                        cancellationToken);
            }

            var now = DateTime.UtcNow;

            if (device == null)
            {
                device = new UserDeviceToken
                {
                    UserId = userId,
                    InstallationId = installationId,
                    FcmToken = fcmToken,
                    TokenHash = tokenHash,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _db.UserDeviceTokens.Add(device);
            }
            else
            {
                device.UserId = userId;
                device.InstallationId = installationId;
                device.FcmToken = fcmToken;
                device.TokenHash = tokenHash;
                device.IsActive = true;
                device.UpdatedAt = now;
            }

            await _db.SaveChangesAsync(cancellationToken);

            return device.Id;
        }

        // Firebase implementation lives in this service now.
        private static async Task<string> SendFirebaseAsync(
            string fcmToken,
            string title,
            string body,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
                throw new ArgumentException("FCM token is required.", nameof(fcmToken));

            var message = new FirebaseAdmin.Messaging.Message
            {
                Token = fcmToken,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = new Dictionary<string, string>
                {
                    { "type", "general" },
                    { "source", "fynd" }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            return await FirebaseMessaging.DefaultInstance.SendAsync(
                message,
                cancellationToken);
        }
    }
}
