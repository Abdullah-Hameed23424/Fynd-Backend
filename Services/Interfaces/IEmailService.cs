
namespace Fynd.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default
    );
}
