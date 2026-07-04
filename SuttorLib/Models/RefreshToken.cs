using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Obsolete("Token is no longer stored in plain text. Use TokenHash instead.")]
        public string? Token { get; set; }

        [Required]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        [NotMapped]
        public bool IsActive => Revoked == null && DateTime.UtcNow <= Expires;
    }
}