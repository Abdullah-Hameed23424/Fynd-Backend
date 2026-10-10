
using FirebaseAdmin.Messaging;
using Fynd.Api.Services.Interfaces;

namespace Fynd.Api.Services
{
    public class FirebaseNotificationService
        : IFirebaseNotificationService
    {
        public async Task<string> SendAsync(
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

            var message = new Message
            {
                Token = fcmToken,

                Notification = new Notification
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
