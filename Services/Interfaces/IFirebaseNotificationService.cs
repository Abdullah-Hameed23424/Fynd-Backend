
namespace Fynd.Api.Services.Interfaces
{
    public interface IFirebaseNotificationService
    {
        Task<string> SendAsync(
            string fcmToken,
            string title,
            string body,
            CancellationToken cancellationToken = default);
    }
}
