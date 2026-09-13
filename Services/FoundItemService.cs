using Fynd.Api.Data;
using Fynd.Api.DTOs.FoundItem;
using Fynd.Api.Models;
using Fynd.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Fynd.Api.Services
{
    public class FoundItemService : IFoundItemService
    {
        private readonly ApplicationDbContext _context;

        public FoundItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FoundItemResponse> CreateAsync(
            int userId,
            CreateFoundItemRequest request)
        {
            FoundItem item = new FoundItem
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Location = request.Location.Trim(),
                FoundAt = request.FoundAt,
                Priority = request.Priority,
                ImageUrl = request.ImageUrl?.Trim(),

                UserId = userId,
                Status = FoundItemStatus.Found,
                CreatedAt = DateTime.UtcNow
            };

            _context.FoundItems.Add(item);

            await _context.SaveChangesAsync();

            return MapToResponse(item);
        }

        public async Task<IEnumerable<FoundItemResponse>> GetAllAsync()
        {
            var items = await _context.FoundItems
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return items.Select(MapToResponse);
        }

        public async Task<FoundItemResponse?> GetByIdAsync(int id)
        {
            var item = await _context.FoundItems
                .AsNoTracking()
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
            UpdateFoundItemRequest request)
        {
            var item = await _context.FoundItems
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
            item.FoundAt = request.FoundAt;
            item.Priority = request.Priority;
            item.ImageUrl = request.ImageUrl?.Trim();

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int userId,
            int id)
        {
            var item = await _context.FoundItems
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

            if (item == null)
            {
                return false;
            }

            _context.FoundItems.Remove(item);

            await _context.SaveChangesAsync();

            return true;
        }

        private static FoundItemResponse MapToResponse(
            FoundItem item)
        {
            return new FoundItemResponse
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Location = item.Location,
                FoundAt = item.FoundAt,
                Status = item.Status,
                Priority = item.Priority,
                CreatedAt = item.CreatedAt,
                UserId = item.UserId,
                ImageUrl = item.ImageUrl
            };
        }
    }
}