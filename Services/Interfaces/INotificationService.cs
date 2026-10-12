
using Fynd.Api.DTOs.Notifications;

namespace Fynd.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task<int> CreateAndSendAsync(
            int userId,
            string title,
            string message,
            string type = "General",
            int? relatedItemId = null,
            string? relatedItemType = null,
            CancellationToken cancellationToken = default);

        Task<NotificationListResponse> GetNotificationsAsync(
            int userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<bool> MarkAsReadAsync(
            int userId,
            int notificationId,
            CancellationToken cancellationToken = default);

        Task<int> MarkAllAsReadAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<string> SendTestAsync(
            string fcmToken,
            string title,
            string body,
            CancellationToken cancellationToken = default);

        Task<int?> RegisterDeviceAsync(
            int userId,
            string fcmToken,
            string installationId,
            CancellationToken cancellationToken = default);
    }
}
