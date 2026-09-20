using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.Models
{
    public class LostItem
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
        public DateTime LostAt { get; set; }

        public LostItemStatus Status { get; set; } = LostItemStatus.Lost;

        public LostItemPriority Priority { get; set; } = LostItemPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}