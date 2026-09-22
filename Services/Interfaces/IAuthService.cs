using Fynd.Api.DTOs.Auth;

namespace Fynd.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse?> LoginAsync(LoginRequest request);

        Task<AuthResponse?> RefreshTokenAsync(
            RefreshTokenRequest request);

        Task<string?> ForgotPasswordAsync(ForgotPasswordRequest request);

        Task<string?> VerifyOtpAsync(VerifyOtpRequest request);

        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);

        Task LogoutAsync(string refreshToken);
    }
}