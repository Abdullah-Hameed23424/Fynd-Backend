
namespace Fynd.Api.DTOs.Notifications
{
    public class NotificationResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int? RelatedItemId { get; set; }

        public string? RelatedItemType { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}
