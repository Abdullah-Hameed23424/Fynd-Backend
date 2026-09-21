using System.ComponentModel.DataAnnotations;
using Fynd.Api.Models;

namespace Fynd.Api.DTOs.LostItem
{
    public class UpdateLostItemRequest
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

        [Required]
        public LostItemPriority Priority { get; set; }

        public int CategoryId { get; set; }

        [Url]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}