
using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.Models
{
    public class UserDeviceToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string InstallationId { get; set; } = string.Empty;

        [Required]
        [MaxLength(4096)]
        public string FcmToken { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
