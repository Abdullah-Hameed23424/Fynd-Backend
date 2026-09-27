using Fynd.Api.DTOs.Home;
using Fynd.Api.Models;

namespace Fynd.Api.DTOs.Posts
{
    public class PostQueryParameters
    {
        public string? Search { get; set; }

        public int? CategoryId { get; set; }

        public PostType? Type { get; set; }

        public string? Location { get; set; }

        public LostItemStatus? LostStatus { get; set; }

        public FoundItemStatus? FoundStatus { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public PostSort Sort { get; set; } = PostSort.Newest;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
