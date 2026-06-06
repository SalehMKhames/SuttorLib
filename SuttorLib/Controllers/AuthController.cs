using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLibrary.Core;
using SuttorLibrary.Core.Services;
using SuttorLibrary.DTOs;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUnitOfWork unit, ILogger<AuthController> logger, IFileService fileService) : ControllerBase
    {
        private readonly IUnitOfWork _unit = unit;
        private readonly ILogger<AuthController> _logger = logger;
        private readonly IFileService _fileService = fileService;

        //POST /api/Auth/Register
        [HttpPost("Register", Name = "Register")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDTO register)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {

                //Save the user's picture to the specified directory in appsettings.json
                string? uploadedPicture = register.userPic is not null ?
                    await _fileService.UploadUserPicAsync(register.userPic!, register.FullName) : null;

                if (string.Equals(uploadedPicture, "A picture with the same name already exists.", StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"The file for '{register.userPic!.FileName}' already exists.");

                var result = await _unit.AuthRepo.RegisterUser(register, uploadedPicture);
                if (result is null)
                {
                    _logger.LogInformation("Registration failed for email {Email}", register.Email);
                    return BadRequest("Email or username already exists.");
                }

                //Check if adding to the database succeeded. If not, delete the uploaded file and return an error.
                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    // Database commit failed — delete the uploaded file
                    _logger.LogError(dbEx, "Database commit failed. Deleting uploaded file: {FileName}", uploadedPicture);
                    await _fileService.DeleteFileAsync(result.PhotoPath!);
                    throw; // Re-throw to be caught by outer catch
                }

                UserDTO userDTO = new UserDTO 
                {
                    Id = result.Id,
                    FullName = result.FullName,
                    Email = result.Email,
                    UserName = result.UserName,
                    Photo = register.userPic,
                    IsAuthor = result.IsAuthor,
                    XP = result.XP,
                    JoinedAt = result.JoinedAt,
                    Token = result.Token,
                    ExpiresAt = result.ExpiresAt,
                    Roles = result.Roles,
                    message = result.message
                };

                return CreatedAtAction(nameof(CreateUser), new { userId = result.Id }, userDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user {Email}", register.Email);
                return Problem("An error occurred during registration.");
            }
        }

        // POST /api/Auth/LogIn
        [HttpPost("LogIn", Name = "LogInUser")]
        public async Task<IActionResult> LogIn([FromBody] LoginDTO login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            try
            {
                var result = await _unit.AuthRepo.LoginUser(login);

                if (result is null)
                {
                    _logger.LogInformation("Login failed for email {Email}", login.Email);
                    return Unauthorized("Invalid email or password.");
                }
                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    // Database commit failed — delete the uploaded file
                    _logger.LogError(dbEx, "Database commit failed. Logging In Function");
                    throw; // Re-throw to be caught by outer catch
                }

                IFormFile? userPic = result.PhotoPath is null ? null :
                    await _fileService.GetPictureAsync(result.PhotoPath);
                
                UserDTO userDTO = new UserDTO 
                {
                    Id = result.Id,
                    FullName = result.FullName,
                    UserName = result.UserName,
                    Email = result.Email,
                    Photo = userPic,
                    JoinedAt = result.JoinedAt,
                    XP = result.XP,
                    IsAuthor = result.IsAuthor,
                    Token = result.Token,
                    ExpiresAt = result.ExpiresAt,
                    message = result.message,
                    Roles = result.Roles
                };

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", login.Email);
                return Problem("An error occurred during login.");
            }
        }

        // PUT /api/Auth/Update/{{userID}}
        [Authorize]
        [HttpPut("update/{userId}", Name = "Update-User")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateDto, [FromRoute] string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if(string.IsNullOrWhiteSpace(userId.ToString()))
                return BadRequest("UserId is required");

            try
            {
                var callerId = User.FindFirst("uid")?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!User.IsInRole("Admin") && !string.Equals(callerId, userId, StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                var userPicPath = updateDto.newCoverPic is not null ? 
                    await _fileService.UploadUserPicAsync(updateDto.newCoverPic!, userId) : null;

                var result = await _unit.AuthRepo.UpdateUser(userId, updateDto.Email, updateDto.UserName, updateDto.FullName, userPicPath, updateDto.XP);
                if (result is null)
                {
                    _logger.LogInformation("Update failed: user not found {UserId}", userId);
                    return NotFound($"User with ID '{userId}' not found.");
                }

                await _unit.CompleteAsync();

                UserDTO userDto = new UserDTO 
                {
                    Id = userId,
                    Email = result.Email,
                    UserName = result.UserName,
                    FullName = result.FullName,
                    IsAuthor = result.IsAuthor,
                    JoinedAt = result.JoinedAt,
                    XP = result.XP,
                    Photo = updateDto.newCoverPic,
                    Token = result.Token,
                    ExpiresAt = result.ExpiresAt,
                    Roles = result.Roles,
                    message = result.message
                };

                return Ok(userDto);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "Invalid operation while updating user {UserId}", userId);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return Problem("An error occurred while updating the user.");
            }
        }

        // PATCH /api/Auth/ChangePassword
        [Authorize]
        [HttpPatch("ChangePassword", Name = "ChangeUserPassword")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var callerEmail = User.FindFirst("email")?.Value ?? User.Identity?.Name;
                if (!User.IsInRole("Admin") 
                        && !string.Equals(callerEmail, dto.Email, StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                var result = await _unit.AuthRepo.ChangePassword(dto);
                await _unit.CompleteAsync();
                return Ok(new { success = true, message = "Password changed successfully." });
            }
            catch (KeyNotFoundException knf)
            {
                _logger.LogInformation(knf, "ChangePassword: user not found {Email}", dto.Email);
                return NotFound(knf.Message);
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogInformation(ua, "ChangePassword: unauthorized for {Email}", dto.Email);
                return Unauthorized(ua.Message);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "ChangePassword: operation failed for {Email}", dto.Email);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for {Email}", dto.Email);
                return Problem("An error occurred while changing the password.");
            }
        }

        //PATCH /api/Auth/AssginNewRole
        [Authorize(Roles = "Admin")]
        [HttpPatch("AssignRole", Name = "AssignNewRole")]
        public async Task<IActionResult> AssignNewRole([FromBody] AssignRoleDTO roleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _unit.AuthRepo.AssignRole(roleDto);
                if (result is null)
                {
                    _logger.LogInformation("AssignRole: user not found for {Email}", roleDto.Email);
                    return NotFound($"User with email '{roleDto.Email}' not found.");
                }

                if (result.StartsWith("Failed"))
                {
                    _logger.LogWarning("AssignRole failed for {Email}: {Message}", roleDto.Email, result);
                    return BadRequest(result);
                }

                await _unit.CompleteAsync();
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {Role} to {Email}", roleDto.Role, roleDto.Email);
                return Problem("An error occurred while assigning the role.");
            }
        }

        // DELETE /api/Auth/DeleteUser/{userId}
        // Admins can delete any user. Non-admins can delete their own account only.
        [Authorize]
        [HttpDelete("DeleteUser/{userId}", Name = "DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid userId, [FromBody] DeleteUserDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (userId == Guid.Empty)
                return BadRequest("User ID is required.");

            try
            {
                var callerId = User.FindFirst("uid")?.Value
                               ?? User.FindFirst("sub")?.Value
                               ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!User.IsInRole("Admin") && !string.Equals(callerId, userId.ToString(), StringComparison.OrdinalIgnoreCase))
                    return Forbid();

                var success = await _unit.AuthRepo.DeleteUser(userId, dto.Password);
                if (!success)
                {
                    _logger.LogInformation("DeleteUser: user not found {UserId}", userId);
                    return NotFound($"User with ID '{userId}' not found.");
                }

                await _unit.CompleteAsync();
                _logger.LogInformation("DeleteUser: user {UserId} deleted by {Caller}", userId, callerId);
                return Ok(new { success = true, message = "User deleted successfully." });
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogInformation(ua, "DeleteUser unauthorized for {UserId}", userId);
                return Unauthorized(ua.Message);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "DeleteUser failed for {UserId}", userId);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user {UserId}", userId);
                return Problem("An error occurred while deleting the user.");
            }
        }


        // POST /api/Auth/Refresh
        [HttpPost("Refresh", Name = "RefreshToken")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var tokens = await _unit.AuthRepo.RefreshTokensAsync(dto.RefreshToken);
                if (tokens is null)
                    return Unauthorized("Invalid or expired refresh token.");

                await _unit.CompleteAsync();
                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return Problem("An error occurred while refreshing token.");
            }
        }

        // POST /api/Auth/Revoke
        [Authorize]
        [HttpPost("Revoke", Name = "RevokeRefreshToken")]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _unit.AuthRepo.RevokeRefreshTokenAsync(dto.RefreshToken);
                if (!result)
                    return NotFound("Refresh token not found or already revoked.");

                await _unit.CompleteAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return Problem("An error occurred while revoking refresh token.");
            }
        }
    }
}
