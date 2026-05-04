using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLibrary.Core;
using SuttorLibrary.Core.Services;
using SuttorLibrary.DTOs;

namespace SuttorLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(
        IUnitOfWork unit, ILogger<UsersController> logger, IFileService fileService
        ) : ControllerBase
    {
        private readonly IUnitOfWork _unit = unit;
        private readonly ILogger<UsersController> _logger = logger;
        private readonly IFileService _fileService = fileService;

        // GET /api/Users/UserByEmail?email=...
        [HttpGet("UserByEmail", Name = "UserByEmail")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            try
            {
                var user = await _unit.UserRepo.GetUserByEmail(email);
                if (user is null)
                    return NotFound($"User with email '{email}' not found.");

                IFormFile? photo = null;
                if (!string.IsNullOrEmpty(user.PhotoPath))
                {
                    photo = await _fileService.GetPictureAsync(user.PhotoPath);
                }

                var result = new
                {
                    Id = (string) user.Id,
                    FullName = (string) user.FullName,
                    UserName = (string) user.UserName,
                    Email = (string) user.Email,
                    XP = (int) user.XP,
                    IsAuthor = (bool) user.IsAuthor,
                    photo
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email}", email);
                return Problem("An error occurred while retrieving the user.");
            }
        }

        // GET /api/Users/UserByUsername?username=...
        [HttpGet("UserByUsername", Name = "UserByUsername")]
        public async Task<IActionResult> GetUserByUsername([FromQuery] string username)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            try
            {
                var user = await _unit.UserRepo.GetUserByUsername(username);
                if (user is null)
                    return NotFound($"User with username '{username}' not found.");

                IFormFile? photo = null;
                if (!string.IsNullOrEmpty(user.PhotoPath))
                {
                    photo = await _fileService.GetPictureAsync(user.PhotoPath);
                }

                var result = new
                {
                    Id = (string)user.Id,
                    FullName = (string)user.FullName,
                    UserName = (string)user.UserName,
                    Email = (string)user.Email,
                    XP = (int)user.XP,
                    IsAuthor = (bool)user.IsAuthor,
                    photo
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                return Problem("An error occurred while retrieving the user.");
            }
        }

        // GET /api/Users/GetAllUsers?page=1&pageSize=50&search=...
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers", Name = "GetAllUsers")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be positive integers.");

            const int maxPageSize = 200;
            pageSize = Math.Min(pageSize, maxPageSize);

            try
            {
                var allUsers = (await _unit.UserRepo.GetAll()).ToList();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLowerInvariant();
                    allUsers = allUsers.Where(u =>
                        (u.Email?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                        (u.UserName?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                        (u.FullName?.ToLowerInvariant().Contains(searchLower) ?? false)
                    ).ToList();
                }

                var total = allUsers.Count;
                var totalPages = (int)Math.Ceiling(total / (double)pageSize);
                var items = allUsers
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Admin requested users page {Page}/{TotalPages} (size {PageSize}) search={Search}",
                    page, totalPages, pageSize, search ?? "none");

                var result = new
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Items = items
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return Problem("An error occurred while retrieving users.");
            }
        }

        //GET /api/Users/User?uid={userId}
        [HttpGet("User")]
        public async Task<IActionResult> GetUser([FromQuery] Guid uid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(uid.ToString()))
                return BadRequest("The user id is required");
            try
            {
                var user = await _unit.UserRepo.GetById(uid.ToString());
                if (user is null)
                    return NotFound($"User with id: '{uid}' not found.");

                IFormFile? photo = null;
                if (!string.IsNullOrEmpty(user.PhotoPath))
                {
                    photo = await _fileService.GetPictureAsync(user.PhotoPath);
                }

                var result = new
                {
                    Id = (string)user.Id,
                    FullName = (string)user.FullName,
                    UserName = (string)user.UserName,
                    Email = (string)user.Email,
                    XP = (int)user.XP,
                    IsAuthor = (bool)user.IsAuthor,
                    photo
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by id {id}", uid);
                return Problem("An error occurred while retrieving the user.");
            }
        }

        //Post /api/Users/AddInterests
        [HttpPost("AddInterests")]
        public async Task<IActionResult> AddInterest([FromBody] UserInterestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.CategoriesNames is null || dto.CategoriesNames.Count == 0)
                return BadRequest("You must add some categories that you are interested in!");

            try {
                var isAdded = await _unit.UserRepo.AddUserInterest(dto);
                if (isAdded == null)
                    return NotFound("User Not Found");
                if (isAdded == false)
                    return BadRequest("Cannot add your interests right now. Try again.");

                await _unit.CompleteAsync();
                return Ok(new { success = true, message = "Your interests have been added." });
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogInformation(ua, "AddInterests unauthorized for {UserId}", dto.UserID);
                return Unauthorized(ua.Message);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "AddInterests failed for {UserId}", dto.UserID);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding interests user {UserId}", dto.UserID);
                return Problem("An error occurred while adding the user's interests.");
            }
        }

        //Patch /api/Users/ChangeInterests
        [HttpPatch("ChangeInterests")]
        public async Task<IActionResult> ChangeInterests([FromBody] UserInterestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.CategoriesNames is null || dto.CategoriesNames.Count == 0)
                return BadRequest("You must add some categories that you are interested in!");

            try
            {
                var updatedCategories = await _unit.UserRepo.UpdateUserInterest(dto);

                if (updatedCategories == null)
                    return NotFound("User not found or no matching categories were provided.");

                await _unit.CompleteAsync();

                var responseCategories = updatedCategories
                    .Select(c => new { Id = c?.Id, Name = c?.Name })
                    .ToList();

                return Ok(new { success = true, message = "Your interests have been modified." });
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogInformation(ua, "AddInterests unauthorized for {UserId}", dto.UserID);
                return Unauthorized(ua.Message);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "AddInterests failed for {UserId}", dto.UserID);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding interests user {UserId}", dto.UserID);
                return Problem("An error occurred while adding the user's interests.");
            }
        }
    }
}
