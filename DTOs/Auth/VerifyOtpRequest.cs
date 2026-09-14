using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string Otp { get; set; } = string.Empty;
    }
}