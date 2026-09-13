using System.ComponentModel.DataAnnotations;

namespace Fynd.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LostItem> LostItems { get; set; } = new List<LostItem>();

        public ICollection<FoundItem> FoundItems { get; set; } = new List<FoundItem>();
    }
}