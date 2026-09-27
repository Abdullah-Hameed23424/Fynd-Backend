namespace Fynd.Api.DTOs.Posts
{
    public class PagedPostResponse
    {
        public IEnumerable<PostResponse> Posts { get; set; } = [];

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}
