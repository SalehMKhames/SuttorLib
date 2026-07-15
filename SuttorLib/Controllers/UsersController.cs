using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.Core.Services.Files;
using SuttorLib.Models.Library;
using SuttorLibrary.Core;
using SuttorLibrary.DTOs;
using System.Security.Claims;
using System.Security.Cryptography;

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

        //GET /api/Users?uid={userId}
        [HttpGet]
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
        [Authorize]
        [HttpPost("AddInterests")]
        public async Task<IActionResult> AddInterest([FromBody] List<string> categories)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categories is null || categories.Count == 0)
                return BadRequest("You must add some categories that you are interested in!");

           var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
           if (userId is null)
                    return Unauthorized("User not found.");
            try {

                var isAdded = await _unit.UserRepo.AddUserInterest(userId, categories);
                if (isAdded == false)
                    return BadRequest("Cannot add your interests right now. Try again.");

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    // Database commit failed — delete the uploaded file
                    _logger.LogError(dbEx, "Database commit failed. Adding User's interests.");
                    throw; // Re-throw to be caught by outer catch
                }

                return Ok(new { success = true, message = "Your interests have been added." });
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogInformation(ua, "AddInterests unauthorized for {UserId}", userId);
                return Unauthorized(ua.Message);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "AddInterests failed for {UserId}", userId);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding interests user {UserId}", userId);
                return Problem("An error occurred while adding the user's interests.");
            }
        }

        //Patch /api/Users/ChangeInterests
        [Authorize]
        [HttpPatch("ChangeInterests")]
        public async Task<IActionResult> ChangeInterests([FromBody] List<string> categories)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categories is null || categories.Count == 0)
                return BadRequest("You must add some categories that you are interested in!");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return Unauthorized();

            try
            {
                var updatedCategories = await _unit.UserRepo.UpdateUserInterest(userId, categories);

                if (updatedCategories == null)
                    return NotFound("User not found or no matching categories were provided.");

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    // Database commit failed — delete the uploaded file
                    _logger.LogError(dbEx, "Database commit failed. Updating User's interests.");
                    throw; // Re-throw to be caught by outer catch
                }

                var responseCategories = updatedCategories
                    .Select(c => new { c?.Id, c?.Name })
                    .ToList();

                return Ok(new { success = true, message = "Your interests have been modified." });
            }
            catch (UnauthorizedAccessException ua)
            {
                _logger.LogInformation(ua, "AddInterests unauthorized for {UserId}", userId);
                return Unauthorized(ua.Message);
            }
            catch (InvalidOperationException io)
            {
                _logger.LogWarning(io, "AddInterests failed for {UserId}", userId);
                return BadRequest(io.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error adding interests user {UserId}", userId);
                return Problem("An error occurred while adding the user's interests.");
            }
        }

        //Patch /api/Users/AddXP?points=...
        [Authorize]
        [HttpPatch("AddXP")]
        public async Task<IActionResult> Promote([FromBody] string uid, [FromQuery] int xp)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (string.IsNullOrEmpty(uid))
                return BadRequest("User ID is required");

            try
            {
                var user = await _unit.UserRepo.GetById(uid.ToString());
                if (user is null)
                    return NotFound($"User with id: '{uid}' not found.");

                var res = await _unit.UserRepo.PromoteToAuthor(user, xp);

                if (!res)
                    return StatusCode(StatusCodes.Status304NotModified);

                return Ok(new { success = true, message = $"Your XP points have been added {xp} points" });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by id {id}", uid);
                return Problem("An error occurred while retrieving the user.");
            }
        }
    }
}
