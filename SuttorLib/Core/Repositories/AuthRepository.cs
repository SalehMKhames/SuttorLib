using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SuttorLibrary.Core.Repositories
{
    public class AuthRepository(
        AppDbContext context,
        UserManager<AppUser> userManager,
        IConfiguration config
        ) : GenericRepo<AppUser>(context), IAuthRepository
    {
        private readonly AppDbContext _context = context;
        private readonly UserManager<AppUser> _userManager = userManager;
        protected readonly IConfiguration _config = config;

        public async Task<bool> ChangePassword(ChangePasswordDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email)
                ?? throw new KeyNotFoundException($"User with email '{dto.Email}' not found.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, dto.OldPassword);
            if (!passwordValid)
                throw new UnauthorizedAccessException("Current password is incorrect.");

            var changeResult = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
            if (!changeResult.Succeeded)
            {
                var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Password change failed: {errors}");
            }

            return true;

        }

        public async Task<AppUser?> LoginUser(LoginDTO login)
        {
            var existedUser = await _userManager.FindByEmailAsync(login.Email!);
            if (existedUser is null || !await _userManager.CheckPasswordAsync(existedUser, login.Password))
                return null;

            var token = await CreateJwtToken(existedUser);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var rolesList = await _userManager.GetRolesAsync(existedUser);
           
            existedUser.Token = jwtToken;
            existedUser.ExpiresAt = token.ValidTo;
            existedUser.Roles = rolesList;
            existedUser.IsAuthed = true;

            return existedUser;
        }

        public async Task<AppUser?> RegisterUser(RegisterDTO register)
        {
            if (await _userManager.FindByEmailAsync(register.Email) is not null)
                    return null;
            if (await _userManager.FindByNameAsync(register.Username) is not null)
                    return null;

            AppUser newUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = register.Username,
                Email = register.Email,
                FullName = register.FullName,
                JoinedAt = DateTime.UtcNow,
                EmailConfirmed = false,
                IsAuthor = false,
                XP = 0,
                PhotoPath = string.Empty
            };
            

            var t = await CreateJwtToken(newUser);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(t);
            newUser.Token = jwtToken;
            newUser.ExpiresAt = t.ValidTo;

            var creatingResult = await _userManager.CreateAsync(newUser, register.Password);
            if (!creatingResult.Succeeded)
            {
                newUser.IsAuthed = false;
                newUser.message = string.Join("; ", creatingResult.Errors.Select(e => e.Description));
                return newUser;
            }

            await _userManager.AddToRoleAsync(newUser, "User");
            return newUser;
        }

        public async Task<AppUser?> UpdateUser(string id, UpdateUserDTO userDTO)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return null;

            if (!string.IsNullOrWhiteSpace(userDTO.Email) && userDTO.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(userDTO.Email) is not null)
                    throw new InvalidOperationException("Email is already in use.");
                user.Email = userDTO.Email;
                user.NormalizedEmail = userDTO.Email.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(userDTO.UserName) && userDTO.UserName != user.UserName)
            {
                if (await _userManager.FindByNameAsync(userDTO.UserName) is not null)
                    throw new InvalidOperationException("Username is already in use.");
                user.UserName = userDTO.UserName;
                user.NormalizedUserName = userDTO.UserName.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(userDTO.FullName))
                user.FullName = userDTO.FullName;

            if (!string.IsNullOrWhiteSpace(userDTO.PhotoPath))
                user.PhotoPath = userDTO.PhotoPath;

            if (userDTO.XP.HasValue && userDTO.XP.Value >= 0)
                user.XP = userDTO.XP.Value;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"User update failed: {errors}");
            }

            return user;
        }

        public async Task<string?> AssignRole(AssignRoleDTO roleDto)
        {
            var user = await _userManager.FindByEmailAsync(roleDto.Email);
            if (user is null)
                return null;

            // Ensure role name is normalized to common format (optional)
            var normalizedRole = roleDto.Role.Trim();

            var result = await _userManager.AddToRoleAsync(user, normalizedRole);
            if (result.Succeeded)
            {
                return $"User '{user.Email}' assigned to role '{normalizedRole}'.";
            }

            // Return combined error descriptions to help client/debugging
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return $"Failed to assign role: {errors}";
        }

        public async Task<bool> DeleteUser(Guid id, string password)
        {
            // AppUser.Id is stored as string (Guid.ToString()), so convert Guid to string
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return false;

            // Verify provided current password
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                throw new UnauthorizedAccessException("Current password is incorrect.");

            // Delete the user record
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Deleting user failed: {errors}");
            }

            return true;
        }





        private async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim> ();

            foreach (var role in roles) {
                roleClaims.Add(new Claim("roles", role));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, 
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), 
                    ClaimValueTypes.Integer64)
            }.Union(userClaims).Union(roleClaims);


            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("JWT:Key")!)
            );
            var signingCred = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer : _config.GetValue<string>("JWT:Issuer"),
                audience : _config.GetValue<string>("JWT:Audience"),
                claims : claims,
                expires : DateTime.UtcNow.AddDays(_config.GetValue<int>("JWT:DurationInDays")),
                signingCredentials : signingCred
            );

            return token;
        }

        public async Task<TokenResponseDTO?> RefreshTokensAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens
                .AsTracking()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (stored is null || !stored.IsActive)
                return null;

            // rotate: revoke existing and create new one
            stored.Revoked = DateTime.UtcNow;

            // create new refresh token
            var newRefreshString = GenerateSecureTokenString(64);
            var refreshExpiresDays = _config.GetValue<int?>("JWT:RefreshTokenDurationDays") ?? 30;
            var newRefresh = new RefreshToken
            {
                Token = newRefreshString,
                UserId = stored.UserId,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(refreshExpiresDays)
            };

            stored.ReplacedByToken = newRefreshString;

            _context.RefreshTokens.Add(newRefresh);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
            if (user is null)
                return null;

            var jwt = await CreateJwtToken(user);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = jwt.ValidTo,
                RefreshToken = newRefreshString,
                RefreshTokenExpiresAt = newRefresh.Expires
            };
        }

        private static string GenerateSecureTokenString(int size = 64)
        {
            var bytes = RandomNumberGenerator.GetBytes(size);
            return Convert.ToBase64String(bytes);
        }

        public async Task<TokenResponseDTO> GenerateTokensAsync(AppUser user)
        {
            // Create access token
            var jwt = await CreateJwtToken(user);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            var accessExpires = jwt.ValidTo;

            // Create refresh token
            var refreshTokenString = GenerateSecureTokenString(64);
            var refreshExpiresDays = _config.GetValue<int?>("JWT:RefreshTokenDurationDays") ?? 30;
            var refreshToken = new RefreshToken
            {
                Token = refreshTokenString,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(refreshExpiresDays),
                Created = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpires,
                RefreshToken = refreshTokenString,
                RefreshTokenExpiresAt = refreshToken.Expires
            };
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var stored = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
            if (stored is null || stored.Revoked != null)
                return false;

            stored.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
