using Fynd.Api.Data;
using Fynd.Api.DTOs.Home;
using Fynd.Api.DTOs.Posts;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<PostResponse>> GetRecentPostsAsync()
        {
            var lostPosts = await _context.LostItems
            .Include(x => x.Category)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new PostResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Location = x.Location,
                Type = PostType.Lost,
                CreatedAt = x.CreatedAt,
                ImageUrl = x.ImageUrl,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Date = x.LostAt,
                UserId = x.UserId
            })
            .ToListAsync();

            var foundPosts = await _context.FoundItems
                .Include(x => x.Category)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new PostResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,
                    Type = PostType.Found,
                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name,
                    Date = x.FoundAt,
                    UserId = x.UserId
                })
                .ToListAsync();

            return lostPosts
                .Concat(foundPosts)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10);
        }
    }
}
