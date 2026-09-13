using Fynd.Api.DTOs.LostItem;

namespace Fynd.Api.Services.Interfaces
{
    public interface ILostItemService
    {
        Task<LostItemResponse> CreateAsync(
            int userId,
            CreateLostItemRequest request);

        Task<IEnumerable<LostItemResponse>> GetAllAsync();

        Task<LostItemResponse?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int userId,
            int id,
            UpdateLostItemRequest request);

        Task<bool> DeleteAsync(
            int userId,
            int id);
    }
}