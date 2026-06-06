using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.Models
{
    [Table("Users")]
    public class AppUser : IdentityUser
    {
        [Key]
        public override string Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;
        
        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        [Required]
        public override string UserName { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        public override string Email { get; set; } = string.Empty;
        public override string PasswordHash { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public int XP { get; set; }
        public string? PhotoPath { get; set; }

        public bool IsAuthor { get; set; }
        
        [NotMapped]
        public string message { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Token { get; set; }
        [NotMapped]
        public List<string> Roles { get; set; } = new();
    }
}
