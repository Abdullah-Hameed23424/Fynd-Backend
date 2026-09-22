using Fynd.Api.DTOs.Home;
using Fynd.Api.Models;

namespace Fynd.Api.DTOs.Posts
{
    public class PostResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public PostType Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

    }
}
