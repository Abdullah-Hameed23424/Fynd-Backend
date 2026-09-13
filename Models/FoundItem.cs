using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.Models
{
    public class FoundItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime FoundAt { get; set; }

        public FoundItemStatus Status { get; set; } = FoundItemStatus.Found;

        public FoundItemPriority Priority { get; set; } = FoundItemPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}