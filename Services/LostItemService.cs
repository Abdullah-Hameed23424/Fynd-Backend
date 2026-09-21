using Fynd.Api.Data;
using Fynd.Api.DTOs.LostItem;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class LostItemService : ILostItemService
    {
        private readonly ApplicationDbContext _context;

        public LostItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LostItemResponse> CreateAsync(
            int userId,
            CreateLostItemRequest request)
        {
            LostItem item = new LostItem
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Location = request.Location.Trim(),
                LostAt = request.LostAt,
                Priority = request.Priority,
                ImageUrl = request.ImageUrl?.Trim(),

                CategoryId = request.CategoryId,
                UserId = userId,
                Status = LostItemStatus.Lost,
                CreatedAt = DateTime.UtcNow
            };

            _context.LostItems.Add(item);

            await _context.SaveChangesAsync();

            await _context.Entry(item)
                .Reference(x => x.Category)
                .LoadAsync();

            return MapToResponse(item);
        }

        public async Task<IEnumerable<LostItemResponse>> GetAllAsync()
        {
            var items = await _context.LostItems
                .AsNoTracking()
                .Include(x => x.Category)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return items.Select(MapToResponse);
        }

        public async Task<LostItemResponse?> GetByIdAsync(int id)
        {
            var item = await _context.LostItems
                .AsNoTracking()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return null;
            }

            return MapToResponse(item);
        }

        public async Task<bool> UpdateAsync(
            int userId,
            int id,
            UpdateLostItemRequest request)
        {
            var item = await _context.LostItems
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (item == null)
            {
                return false;
            }

            item.Title = request.Title.Trim();
            item.Description = request.Description.Trim();
            item.Location = request.Location.Trim();
            item.LostAt = request.LostAt;
            item.Priority = request.Priority;
            item.ImageUrl = request.ImageUrl?.Trim();
            item.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(
            int userId,
            int id)
        {
            var item = await _context.LostItems
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (item == null)
            {
                return false;
            }

            _context.LostItems.Remove(item);

            await _context.SaveChangesAsync();

            return true;
        }

        private static LostItemResponse MapToResponse(
            LostItem item)
        {
            return new LostItemResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Location = item.Location,
                LostAt = item.LostAt,
                Status = item.Status,
                Priority = item.Priority,
                CreatedAt = item.CreatedAt,
                UserId = item.UserId,
                ImageUrl = item.ImageUrl,
                CategoryId = item.CategoryId,
                CategoryName = item.Category.Name
            };
        }
    }
}