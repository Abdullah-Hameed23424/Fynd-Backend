using Fynd.Api.Models;

namespace Fynd.Api.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string HashRefreshToken(string refreshToken);
    }
}