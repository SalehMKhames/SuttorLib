using SuttorLibrary.Models;
using SuttorLibrary.DTOs;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IAuthRepository : IGenericRepo<AppUser>
    {
        public Task<AppUser?> RegisterUser(RegisterDTO register, string? picName);
        public Task<AppUser?> LoginUser(LoginDTO login);
        public Task<bool> ChangePassword(ChangePasswordDTO passwordDTO);
        public Task<AppUser?> UpdateUser(string id, string? email, string? username, string? fullName, string? picPath, int? xp);
        public Task<bool> DeleteUser(Guid id, string password);
        public Task<string?> AssignRole(AssignRoleDTO roleDto);


        // Refresh token related
        public Task<TokenResponseDTO> GenerateTokensAsync(AppUser user);
        public Task<TokenResponseDTO?> RefreshTokensAsync(string refreshToken);
        public Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
