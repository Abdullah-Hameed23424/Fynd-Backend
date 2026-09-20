using Fynd.Api.DTOs.Catrgory;

namespace Fynd.Api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllAsync();
    }
}
