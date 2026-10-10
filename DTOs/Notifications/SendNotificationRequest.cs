
namespace Fynd.Api.DTOs.Notifications
{
    public class SendNotificationRequest
    {
        public string FcmToken { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;
    }
}
