
using Microsoft.Extensions.Configuration;
using Resend;
using System.Net;

namespace Fynd.Api.Services;

public class EmailService : Interfaces.IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;

    public EmailService(
        IResend resend,
        IConfiguration configuration)
    {
        _resend = resend;
        _configuration = configuration;
    }

    public async Task SendOtpAsync(
        string email,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var fromEmail =
            _configuration["Resend:FromEmail"];

        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "Resend:FromEmail is missing.");
        }

        var message = new EmailMessage
        {
            From = fromEmail,
            Subject = "Fynd - Email Verification",
            HtmlBody = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:Arial,sans-serif">
                    <h2>Verify your Fynd account</h2>
                    <p>Your verification code is:</p>
                    <h1 style="letter-spacing:8px">
                        {WebUtility.HtmlEncode(otp)}
                    </h1>
                    <p>This code expires in 5 minutes.</p>
                    <p>If you did not request this code, ignore this email.</p>
                </body>
                </html>
                """
        };

        message.To.Add(email);

        await _resend.EmailSendAsync(message);
    }
}
