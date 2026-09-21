using Fynd.Api.Data;
using Fynd.Api.DTOs.Catrgory;
using Fynd.Api.DTOs.Home;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _context;

        public HomeService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<HomeResponse> GetHomeAsync()
        {
            var lostItems = await _context.LostItems
                .Include(x => x.Category)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new RecentPosts
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,
                    Type = PostType.Lost,
                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name
                })
                .ToListAsync();

            var foundItems = await _context.FoundItems
                .Include(x => x.Category)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new RecentPosts
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Location = x.Location,
                    Type = PostType.Found,
                    CreatedAt = x.CreatedAt,
                    ImageUrl = x.ImageUrl,
                    CategoryId = x.CategoryId,
                    CategoryName = x.Category.Name
                })
                .ToListAsync();

            var recentPosts = lostItems
                .Concat(foundItems)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .ToList();

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new CategoryResponse
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            return new HomeResponse
            {
                RecentPosts = recentPosts,
                Categories = categories
            };
        }
    }
}
