using Fynd.Api.DTOs.FoundItem;


namespace Fynd.Api.Services.Interfaces
{
    public interface IFoundItemService
    {
        Task<FoundItemResponse> CreateAsync(int userId, CreateFoundItemRequest request);
        Task<IEnumerable<FoundItemResponse>> GetAllAsync();

        Task<FoundItemResponse?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int userId,
            int id,
            UpdateFoundItemRequest request);

        Task<bool> DeleteAsync(
            int userId,
            int id);
    }
}