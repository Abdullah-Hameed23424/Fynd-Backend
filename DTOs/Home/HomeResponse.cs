using Fynd.Api.DTOs.Catrgory;

namespace Fynd.Api.DTOs.Home
{
    public class HomeResponse
    {
        public List<RecentPosts> RecentPosts { get; set; } = [];

        public IEnumerable<CategoryResponse> Categories { get; set; } = [];
    }
}