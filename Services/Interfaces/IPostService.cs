using Fynd.Api.DTOs.Posts;

namespace Fynd.Api.Services.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponse>> GetRecentPostsAsync();
    }
}
