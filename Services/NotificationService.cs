
using System.Security.Cryptography;
using System.Text;
using Fynd.Api.Data;
using Fynd.Api.DTOs.Notifications;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFirebaseNotificationService _firebaseNotificationService;

        public NotificationService(
            ApplicationDbContext db,
            IFirebaseNotificationService firebaseNotificationService)
        {
            _db = db;
            _firebaseNotificationService = firebaseNotificationService;
        }

        public async Task<int> CreateAndSendAsync(
            int userId,
            string title,
            string message,
            string type = "General",
            int? relatedItemId = null,
            string? relatedItemType = null,
            CancellationToken cancellationToken = default)
        {
            var userExists = await _db.Users
                .AnyAsync(x => x.Id == userId, cancellationToken);

            if (!userExists)
            {
                throw new ArgumentException("User not found.", nameof(userId));
            }

            var notification = new Models.AppNotification
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

            // أولاً نحفظ الإشعار في قاعدة البيانات.
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);

            // بعدها نرسل Push للأجهزة المسجلة للمستخدم.
            var devices = await _db.UserDeviceTokens
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var device in devices)
            {
                try
                {
                    await _firebaseNotificationService.SendAsync(
                        device.FcmToken,
                        title,
                        message,
                        cancellationToken);
                }
                catch (FirebaseAdmin.Messaging.FirebaseMessagingException)
                {
                    // فشل إرسال Push لا يلغي الإشعار المحفوظ في قاعدة البيانات.
                    // يمكن إضافة logging ومعالجة التوكنات غير الصالحة لاحقاً.
                }
            }

            return notification.Id;
        }

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

        public async Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Notifications
                .CountAsync(
                    x => x.UserId == userId && !x.IsRead,
                    cancellationToken);
        }

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
            {
                return false;
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;

                await _db.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

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

        public async Task<string> SendTestAsync(
            string fcmToken,
            string title,
            string body,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
            {
                throw new ArgumentException(
                    "FCM token is required.",
                    nameof(fcmToken));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(
                    "Notification title is required.",
                    nameof(title));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentException(
                    "Notification body is required.",
                    nameof(body));
            }

            return await _firebaseNotificationService.SendAsync(
                fcmToken,
                title,
                body,
                cancellationToken);
        }

        public async Task<int?> RegisterDeviceAsync(
            int userId,
            string fcmToken,
            string installationId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
            {
                throw new ArgumentException(
                    "FCM token is required.",
                    nameof(fcmToken));
            }

            if (string.IsNullOrWhiteSpace(installationId))
            {
                throw new ArgumentException(
                    "Installation ID is required.",
                    nameof(installationId));
            }

            if (installationId.Length > 100)
            {
                throw new ArgumentException(
                    "Installation ID cannot exceed 100 characters.",
                    nameof(installationId));
            }

            var userExists = await _db.Users
                .AnyAsync(x => x.Id == userId, cancellationToken);

            if (!userExists)
            {
                return null;
            }

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(fcmToken)));

            // Prefer matching the installation, then the token hash.
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
                device = new Models.UserDeviceToken
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
    }
}
