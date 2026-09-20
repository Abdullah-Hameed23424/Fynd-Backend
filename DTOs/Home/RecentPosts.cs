

namespace Fynd.Api.DTOs.Home
{
    public class RecentPosts
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public PostType Type { get; set; } = PostType.Lost;

        public DateTime CreatedAt { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }
    }
}