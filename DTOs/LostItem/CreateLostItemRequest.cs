using System.ComponentModel.DataAnnotations;
using Fynd.Api.Models;

namespace Fynd.Api.DTOs.LostItem
{
    public class CreateLostItemRequest
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
        public DateTime LostAt { get; set; }

        public LostItemPriority Priority { get; set; }
            = LostItemPriority.Medium;

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}