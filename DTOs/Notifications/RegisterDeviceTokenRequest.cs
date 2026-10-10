
using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.DTOs.Notifications
{
    public class RegisterDeviceTokenRequest
    {
        [Required]
        [MaxLength(4096)]
        public string FcmToken { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string InstallationId { get; set; } = string.Empty;
    }
}
