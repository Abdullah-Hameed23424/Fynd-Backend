using Fynd.Api.DTOs.Home;

namespace Fynd.Api.DTOs.Posts
{
    public class PostDetailsQueryParameters
    {
        public int Id { get; set; }

        public PostType? Type { get; set; }
    }
}
