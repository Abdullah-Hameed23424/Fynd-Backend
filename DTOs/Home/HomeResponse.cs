using Fynd.Api.DTOs.Catrgory;
using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.DTOs.LostItem;

namespace Fynd.Api.DTOs.Home
{
    public class HomeResponse
    {
        public List<RecentPosts> RecentPosts { get; set; } = [];

        public IEnumerable<CategoryResponse> Categories { get; set; } = [];
    }
}