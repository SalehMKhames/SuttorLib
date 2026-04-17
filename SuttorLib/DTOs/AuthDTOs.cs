using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuttorLibrary.DTOs
{
    public class RegisterDTO 
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [MinLength(3, ErrorMessage = "Full name must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Full name cannot exceed 50 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginDTO 
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserDTO
    {
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string? Email { get; set; }

        [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string? UserName { get; set; }

        [MinLength(3, ErrorMessage = "Full name must be at least 3 characters.")]
        [MaxLength(50, ErrorMessage = "Full name cannot exceed 50 characters.")]
        public string? FullName { get; set; }

        public string? PhotoPath { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "XP cannot be negative.")]
        public int? XP { get; set; }
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class AssignRoleDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        [MinLength(2, ErrorMessage = "Role name must be at least 2 characters.")]
        public string Role { get; set; } = string.Empty;
    }

    public class DeleteUserDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class TokenResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
    }

    public class RefreshRequestDTO
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RevokeRequestDTO
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class GetUserDTO
    {
        public string Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        public string FullName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        [MaxLength(50)]
        [Required]
        public string UserName { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
        public int XP { get; set; }
        public string? PhotoPath { get; set; }

        public bool IsAuthor { get; set; }
    }
}