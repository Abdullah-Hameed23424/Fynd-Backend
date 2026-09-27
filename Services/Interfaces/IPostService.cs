using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.DTOs.Home;
using Fynd.Api.DTOs.Posts;

namespace Fynd.Api.Services.Interfaces
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponse>> GetRecentPostsAsync();

        Task<PagedPostResponse> GetPostsAsync(PostQueryParameters parameters);

        Task<PostResponse?> GetPostDetailsAsync(PostDetailsQueryParameters parameters);

        Task<PostResponse> CreatePostAsync(int userId, CreatePostRequest request);

        Task<PagedPostResponse> GetMyPostsAsync(int userId, PostQueryParameters parameters);

        Task<bool> UpdatePostAsync(
           int userId,
           int id,
           UpdatePostRequest request);

        Task<bool> DeletePostAsync(
            PostType postType,
            int userId,
            int id);
    }
}
