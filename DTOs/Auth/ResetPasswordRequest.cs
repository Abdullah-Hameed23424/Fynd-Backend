using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}