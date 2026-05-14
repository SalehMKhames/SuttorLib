using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }
        public string? ReplacedByToken { get; set; }

        [NotMapped]
        public bool IsActive => Revoked == null && DateTime.UtcNow <= Expires;
    }
}