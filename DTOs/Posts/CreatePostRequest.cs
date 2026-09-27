using Fynd.Api.DTOs.Home;
using Fynd.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.DTOs.Posts
{
    public class CreatePostRequest
    {
        public PostType Type { get; set; }

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
        public DateTime Date { get; set; }

        public LostItemPriority? LostPriority { get; set; }
            = LostItemPriority.Medium;

        public FoundItemPriority? FoundPriority { get; set; }
            = FoundItemPriority.Medium;

        public int CategoryId { get; set; }

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}
