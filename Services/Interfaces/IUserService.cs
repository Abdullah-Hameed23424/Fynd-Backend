using Fynd.Api.DTOs.User;
using Fynd.Api.Models;

namespace Fynd.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);

        Task<bool> UpdateProfileAsync(
            int userId,
            UpdateUserRequest request);

        Task<bool> DeleteMyAccountAsync(int userId);
    }
}