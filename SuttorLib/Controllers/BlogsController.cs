using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.Core.Services.Blog;
using SuttorLib.DTOs;
using SuttorLibrary.Core;
using SuttorLibrary.Core.Services;
using SuttorLibrary.Models;
using System.Security.Claims;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogsController(ILogger<BlogsController> logger, 
        IBlogServices blogService, IUnitOfWork unit, IFileService fileService) : ControllerBase
    {
        private readonly ILogger<BlogsController> _logger = logger;
        private readonly IBlogServices _blogService = blogService;
        private readonly IUnitOfWork _unit = unit;
        private readonly IFileService _fileService = fileService;

        // ================== BLOG OPERATIONS ==================

        // api/Blog/Create
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDTO createDTO)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if(string.IsNullOrEmpty(createDTO.Title) || string.IsNullOrEmpty(createDTO.Content))
                return BadRequest("Title and content are required.");

            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                try {
                    var categories = await _unit.BookRepo.GetCategories();
                    var category = categories!.Where(C => C!.Name == createDTO.Category).FirstOrDefault();

                    if (category is not null)
                        createDTO.Category = category.Name;
                    else
                    {
                        category = await _unit.BookRepo.AddCategory(createDTO.Category);
                        createDTO.Category = category!.Name;
                    }

                    try
                    {
                        await _unit.CompleteAsync();
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Failed to insert new Category using Blog Create Function: {Category}", createDTO.Category);
                        throw; // Re-throw to be caught by outer catch
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot add the Category using Blog creating function: {Category}", createDTO.Category);
                    return Problem("An error occurred while adding the category.");
                }

                var blog = await _blogService.CreateBlogAsync(userId, createDTO);

                _logger.LogInformation("Blog uploaded successfully: {Title}", createDTO.Title);

                return CreatedAtAction(nameof(CreateBlog), new { blog.Id, blog.Title}, blog.Title);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the blog.");
                return StatusCode(500, "An error occurred while creating the blog.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogByID(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _blogService.GetBlogById(id);
                if (result is null)
                    return NotFound("There is No Blogs");

                var user = await _unit.UserRepo.GetById(result.PublisherId);
                string? userName = null, userFullName = null;
                IFormFile? userPic = null;

                if (user is null)
                    result.PublisherId = "Unknown Publisher";
                else {
                    userName = user.UserName; 
                    userFullName = user.FullName;

                    userPic = user.PhotoPath is null ? 
                        null : await _fileService.GetPictureAsync(user.PhotoPath);
                }

                var blogDTO = new BlogDTO 
                {
                    Id = result.Id.ToString(),
                    Title = result.Title,
                    Content = result.Content,
                    CreatedAt = result.CreatedAt,
                    publisherId = result.PublisherId,
                    publisherName = userFullName,
                    publisherUserName = userName,
                    publisherPic = userPic,
                    Tags = result.Tags,
                    Likes = result.Likes,
                    Dislikes = result.Dislikes,
                    Views = result.Views,
                };
                
                var categories = await _unit.BookRepo.GetCategories();
                Category category = categories!.FirstOrDefault(c => c!.Id == result.CategoryId)!;
                if (category is null)
                    blogDTO.Category = "";
                else
                    blogDTO.Category = category.Name;

                var comments = await _blogService.GetCommentsAsync(blogDTO.Id);
                if (comments is null || comments.Count == 0)
                    blogDTO.Comments = [];
                else
                    blogDTO.Comments = comments;

                _logger.LogInformation("Blogs retrieved Successfully.");

                return Ok(blogDTO);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the blog that has the ID: {id}.", id);
                return StatusCode(500, "An error occurred while getting the blog.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogs(BlogFilterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _blogService.GetAllBlogs(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }
    }
}
