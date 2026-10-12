
using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.Models
{
    public class AppNotification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "General";

        // Optional identifier of the related post or item.
        public int? RelatedItemId { get; set; }

        // Examples: LostItem, FoundItem, Post.
        [MaxLength(50)]
        public string? RelatedItemType { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public User User { get; set; } = null!;
    }
}
