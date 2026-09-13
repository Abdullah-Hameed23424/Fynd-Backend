using Fynd.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.DTOs.FoundItem
{
    public class CreateFoundItemRequest
    {
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

        public FoundItemPriority Priority { get; set; } = FoundItemPriority.Medium;

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}
