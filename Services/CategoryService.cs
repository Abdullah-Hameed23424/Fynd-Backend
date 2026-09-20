using Fynd.Api.Data;
using Fynd.Api.DTOs.Catrgory;
using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }
        async public Task<IEnumerable<CategoryResponse>> GetAllAsync()
        {
            var items = await _context.Categories
             .AsNoTracking()
             .OrderBy(x => x.Name)
             .ToListAsync();

            return items.Select(MapToResponse);
        }

        private static CategoryResponse MapToResponse(
            Category item)
        {
            return new CategoryResponse
            {
                Id = item.Id,
                Name = item.Name,
            };
        }
    }
}
