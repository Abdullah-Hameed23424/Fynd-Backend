using Fynd.Api.Models;

namespace Fynd.Api.DTOs.LostItem
{
    public class LostItemResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime LostAt { get; set; }

        public LostItemStatus Status { get; set; }

        public LostItemPriority Priority { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }
}