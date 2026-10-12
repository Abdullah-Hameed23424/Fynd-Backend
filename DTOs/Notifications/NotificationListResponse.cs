
namespace Fynd.Api.DTOs.Notifications
{
    public class NotificationListResponse
    {
        public List<NotificationResponse> Notifications { get; set; }
            = new();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int UnreadCount { get; set; }

        public int TotalPages { get; set; }
    }
}
