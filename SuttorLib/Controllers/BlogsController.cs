using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
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

        // Post api/Blog/Create
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateBlog([FromBody] CreateBlogDTO createDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(createDTO.Title) || string.IsNullOrEmpty(createDTO.Content))
                return BadRequest("Title and content are required.");

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                try
                {
                    var categories = await _unit.BookRepo.GetCategories();
                    var category = categories!.Where(C => C!.Name == createDTO.Category).FirstOrDefault();

                    if (category is not null)
                        createDTO.Category = category.Name;
                    else if (createDTO.Category == "" || string.IsNullOrEmpty(createDTO.Category))
                        createDTO.Category = "";

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

                return CreatedAtAction(nameof(CreateBlog), new { blog.Id, blog.Title }, blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the blog.");
                return StatusCode(500, "An error occurred while creating the blog.");
            }
        }

        // GET api/Blog/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlogByID(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(id))
                return BadRequest("The Blog id is required.");

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
                Category category = categories!.FirstOrDefault(c => c!.Id == result.Category)!;
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

        // GET api/Blog/
        [HttpGet()]
        public async Task<IActionResult> GetAllBlogs([FromQuery] int Page = 1,
            [FromQuery] int PageSize = 10,
            [FromQuery] string SortBy = "recent",
            [FromQuery] string? SearchItem = null,
            [FromQuery] string? Tag = null,
            [FromQuery] string? PublisherId = null
         )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new BlogFilterDto {
                Page = Page,
                PageSize = PageSize,
                SearchTerm = SearchItem,
                PublisherId = PublisherId,
                Tag = Tag,
                SortBy = SortBy
            };

            try
            {
                var result = await _blogService.GetAllBlogs(dto);
                if (result is null || result.TotalCount == 0)
                    return NotFound("There is no blogs.");

                ///TODO: Convert to BlogDTO and return it with result instead of result.Item.
                List<BlogDTO> blogs = new List<BlogDTO>();
                foreach (var item in result.Items) 
                {
                    var user = await _unit.UserRepo.GetById(item.PublisherId);
                    string? userName = null, userFullName = null;
                    IFormFile? userPic = null;

                    if (user is null)
                        item.PublisherId = "Unknown Publisher";
                    else
                    {
                        userName = user.UserName;
                        userFullName = user.FullName;

                        userPic = user.PhotoPath is null ?
                            null : await _fileService.GetPictureAsync(user.PhotoPath);
                    }

                    var blogDTO = new BlogDTO
                    {
                        Id = item.Id.ToString(),
                        Title = item.Title,
                        Content = item.Content,
                        CreatedAt = item.CreatedAt,
                        publisherId = item.PublisherId,
                        publisherName = userFullName,
                        publisherUserName = userName,
                        publisherPic = userPic,
                        Tags = item.Tags,
                        Likes = item.Likes,
                        Dislikes = item.Dislikes,
                        Views = item.Views,
                    };

                    var categories = await _unit.BookRepo.GetCategories();
                    Category cat = categories!.FirstOrDefault(c => c!.Id == item.Category)!;
                    if (cat is null)
                        blogDTO.Category = "";
                    else
                        blogDTO.Category = cat.Name;

                    var comments = await _blogService.GetCommentsAsync(blogDTO.Id);
                    if (comments is null || comments.Count == 0)
                        blogDTO.Comments = [];
                    else
                        blogDTO.Comments = comments;

                    blogs.Add(blogDTO);
                }

                return Ok(new { blogs, result.TotalCount, result.TotalPages, result.PageSize});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }

        // PATCH /api/Blog/{id}/update
        [Authorize]
        [HttpPatch("{id}/update")]
        public async Task<IActionResult> UpdateBlog([FromRoute] string blogId, [FromBody] UpdateBlogDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(blogId))
                return BadRequest("The Blog ID is required");
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                try
                {
                    var categories = await _unit.BookRepo.GetCategories();
                    var category = categories!.Where(C => C!.Name == dto.Category).FirstOrDefault();

                    if (category is not null)
                        dto.Category = category.Name;
                    else if (dto.Category == "" || string.IsNullOrEmpty(dto.Category))
                        dto.Category = "";

                    else
                    {
                        category = await _unit.BookRepo.AddCategory(dto.Category);
                        dto.Category = category!.Name;
                    }

                    try
                    {
                        await _unit.CompleteAsync();
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Failed to insert new Category using Blog Update Function: {Category}", dto.Category);
                        throw; // Re-throw to be caught by outer catch
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot add the Category using Blog updating function: {Category}", dto.Category);
                    return Problem("An error occurred while adding the category.");
                }

                var blog = await _blogService.UpdateBlogAsync(blogId, dto, userId);

                var user = await _unit.UserRepo.GetById(blog.PublisherId);
                string? userName = null, userFullName = null;
                IFormFile? userPic = null;

                if (user is null)
                    blog.PublisherId = "Unknown Publisher";
                else
                {
                    userName = user.UserName;
                    userFullName = user.FullName;

                    userPic = user.PhotoPath is null ?
                        null : await _fileService.GetPictureAsync(user.PhotoPath);
                }

                var blogDTO = new BlogDTO
                {
                    Id = blog.Id.ToString(),
                    Title = blog.Title,
                    Content = blog.Content,
                    CreatedAt = blog.CreatedAt,
                    publisherId = blog.PublisherId,
                    publisherName = userFullName,
                    publisherUserName = userName,
                    publisherPic = userPic,
                    Tags = blog.Tags,
                    Likes = blog.Likes,
                    Dislikes = blog.Dislikes,
                    Views = blog.Views,
                    Category = blog.Category
                };

                var comments = await _blogService.GetCommentsAsync(blogId);
                if (comments is null || comments.Count == 0)
                    blogDTO.Comments = [];
                else
                    blogDTO.Comments = comments;

                return Ok(blog);

            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }

        // DELETE api/Blog/{id}/delete
        [Authorize]
        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> DeleteBlog([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(id))
                return BadRequest("The Blog id is required");

            try {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var isDel = await _blogService.DeleteBlogAsync(id, userId);
                if (!isDel) {
                    _logger.LogInformation("DeleteBlog: blog not found {id}", id);
                    return NotFound($"Blog with ID '{id}' not found.");
                }

                _logger.LogInformation("DeleteBlog: blog with the {id} deleted by {Caller}", id, userId);
                return Ok(new { success = true, message = "User deleted successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Blog not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }

        // GET api/Blog/ByCategory?category=...&page=...&pageSize=...
        [HttpGet("ByCategory")]
        public async Task<IActionResult> GetBlogsByCategory([FromQuery] string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(category))
                return BadRequest("Caregory Name is required");

            try {
                var res = await _blogService.GetBlogsByCategory(category, page, pageSize);
                if (res is null || res.TotalCount == 0)
                    return NotFound("Sorry, There is no blogs for this category");

                List<BlogDTO> blogs = new List<BlogDTO>();

                foreach (var item in res.Items)
                {
                    var user = await _unit.UserRepo.GetById(item.PublisherId);
                    string? userName = null, userFullName = null;
                    IFormFile? userPic = null;

                    if (user is null)
                        item.PublisherId = "Unknown Publisher";
                    else
                    {
                        userName = user.UserName;
                        userFullName = user.FullName;

                        userPic = user.PhotoPath is null ?
                            null : await _fileService.GetPictureAsync(user.PhotoPath);
                    }

                    var blogDTO = new BlogDTO
                    {
                        Id = item.Id.ToString(),
                        Title = item.Title,
                        Content = item.Content,
                        CreatedAt = item.CreatedAt,
                        publisherId = item.PublisherId,
                        publisherName = userFullName,
                        publisherUserName = userName,
                        publisherPic = userPic,
                        Tags = item.Tags,
                        Likes = item.Likes,
                        Dislikes = item.Dislikes,
                        Views = item.Views,
                    };

                    var categories = await _unit.BookRepo.GetCategories();
                    Category cat = categories!.FirstOrDefault(c => c!.Id == item.Category)!;
                    if (cat is null)
                        blogDTO.Category = "";
                    else
                        blogDTO.Category = cat.Name;

                    var comments = await _blogService.GetCommentsAsync(blogDTO.Id);
                    if (comments is null || comments.Count == 0)
                        blogDTO.Comments = [];
                    else
                        blogDTO.Comments = comments;

                    blogs.Add(blogDTO);
                }

                _logger.LogInformation("The requested books by category page {Page}/{TotalPages} (size {PageSize})",
                    page, res.TotalPages, pageSize);

                return Ok(new { blogs, res.TotalCount, res.TotalPages, res.PageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs by category.");
                return StatusCode(500, "An error occurred while fetching all blogs by category.");
            }
        }

        // GET /api/Blog/ByPublisher?user=...&page=...&pageSize=...
        [HttpGet("ByPublisher")]
        public async Task<IActionResult> GetBlogsByPublisher([FromQuery] string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Caregory Name is required");

            try
            {
                var res = await _blogService.GetBlogsByCategory(userId, page, pageSize);
                if (res is null || res.TotalCount == 0)
                    return NotFound("Sorry, There is no blogs for this category");

                List<BlogDTO> blogs = new List<BlogDTO>();

                foreach (var item in res.Items)
                {
                    var user = await _unit.UserRepo.GetById(userId);

                    string? userName = user!.UserName, userFullName = user.FullName;

                    IFormFile? userPic = user.PhotoPath is null ?
                            null : await _fileService.GetPictureAsync(user.PhotoPath);

                    var blogDTO = new BlogDTO
                    {
                        Id = item.Id.ToString(),
                        Title = item.Title,
                        Content = item.Content,
                        CreatedAt = item.CreatedAt,
                        publisherId = item.PublisherId,
                        publisherName = userFullName,
                        publisherUserName = userName,
                        publisherPic = userPic,
                        Tags = item.Tags,
                        Likes = item.Likes,
                        Dislikes = item.Dislikes,
                        Views = item.Views,
                    };

                    var categories = await _unit.BookRepo.GetCategories();
                    Category cat = categories!.FirstOrDefault(c => c!.Id == item.Category)!;
                    if (cat is null)
                        blogDTO.Category = "";
                    else
                        blogDTO.Category = cat.Name;

                    var comments = await _blogService.GetCommentsAsync(blogDTO.Id);
                    if (comments is null || comments.Count == 0)
                        blogDTO.Comments = [];
                    else
                        blogDTO.Comments = comments;

                    blogs.Add(blogDTO);
                }

                _logger.LogInformation("The requested books by user with the ID: {id}. page {Page}/{TotalPages} (size {PageSize})",
                    userId, page, res.TotalPages, pageSize);

                return Ok(new { blogs, res.TotalCount, res.TotalPages, res.PageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }

        // GET api/Blog/SearchTag?tag=...&page=...&pageSize=...
        [HttpGet("SearchTag")]
        public async Task<IActionResult> GetBlogsByTag([FromQuery] List<string> tags, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (tags == null || tags.Count == 0)
                return BadRequest("At least one tag is required");

            try {
                var res = await _blogService.SearchBlogsByTags(tags, page, pageSize);
                if (res is null || res.TotalCount == 0)
                    return NotFound("Sorry, There is no blogs for this tag");

                List<BlogDTO> blogs = new List<BlogDTO>();

                foreach (var item in res.Items)
                {
                    var user = await _unit.UserRepo.GetById(item.PublisherId);

                    string? userName = user!.UserName, userFullName = user.FullName;

                    IFormFile? userPic = user.PhotoPath is null ?
                            null : await _fileService.GetPictureAsync(user.PhotoPath);

                    var blogDTO = new BlogDTO
                    {
                        Id = item.Id.ToString(),
                        Title = item.Title,
                        Content = item.Content,
                        CreatedAt = item.CreatedAt,
                        publisherId = item.PublisherId,
                        publisherName = userFullName,
                        publisherUserName = userName,
                        publisherPic = userPic,
                        Tags = item.Tags,
                        Likes = item.Likes,
                        Dislikes = item.Dislikes,
                        Views = item.Views,
                    };

                    var categories = await _unit.BookRepo.GetCategories();
                    Category cat = categories!.FirstOrDefault(c => c!.Id == item.Category)!;
                    if (cat is null)
                        blogDTO.Category = "";
                    else
                        blogDTO.Category = cat.Name;

                    var comments = await _blogService.GetCommentsAsync(blogDTO.Id);
                    if (comments is null || comments.Count == 0)
                        blogDTO.Comments = [];
                    else
                        blogDTO.Comments = comments;

                    blogs.Add(blogDTO);
                }

                _logger.LogInformation("The requested blog with the Tags: {tags}. page {Page}/{TotalPages} (size {PageSize})",
                    tags, page, res.TotalPages, pageSize);

                return Ok(new { blogs, res.TotalCount, res.TotalPages, res.PageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }


        // ================== Comment Operations ====================

        // POST /api/Blog/{blogId}/addComment
        [Authorize]
        [HttpPost("{blogId}/addComment")]
        public async Task<IActionResult> CreateComment([FromRoute] string blogId, [FromBody] CreateCommentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(blogId))
                return BadRequest("Blog Id is required");
            if (string.IsNullOrEmpty(dto.Content))
                return BadRequest("Comment content is required");

            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var res = await _blogService.CreateCommentAsync(blogId, userId, dto);
                if (res is null)
                    return BadRequest("Something went wrong in creating your feed! Please, try again later.");

                _logger.LogInformation("Comment uploaded successfully: {id}", res.Id);

                return CreatedAtAction(nameof(CreateComment), new { res.Id, res.Content }, res);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all blogs.");
                return StatusCode(500, "An error occurred while fetching all blogs.");
            }
        }

        // GET /api/Blog/{blogId}/comments
        //[HttpGet("{blogId}/comments")]
        //public async Task<IActionResult> GetAllComments([FromRoute] string blogId)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    if (string.IsNullOrEmpty(blogId))
        //        return BadRequest("Blog Id is required");

        //    try {
        //        var result = await _blogService.GetCommentsAsync(blogId);

        //        if (result is null || result.Count == 0)
        //            return NotFound("No comments for this blog.");

        //        List<CommentDTO> coms = new List<CommentDTO>();
        //        foreach (var item in result)
        //        {
        //            var user = await _unit.UserRepo.GetById(item.CommenterId);
        //            string? userName = null, userFullName = null;
        //            IFormFile? userPic = null;

        //            if (user is null)
        //                item.CommenterId = "Unknown Publisher";
        //            else
        //            {
        //                userName = user.UserName;
        //                userFullName = user.FullName;

        //                userPic = user.PhotoPath is null ?
        //                    null : await _fileService.GetPictureAsync(user.PhotoPath);
        //            }

        //            var com = new CommentDTO
        //            {
        //                Id = item.Id,
        //                Content = item.Content,
        //                CreatedAt = item.CreatedAt,
        //                CommenterId = item.CommenterId,
        //                CommenterFullName = userFullName,
        //                CommenterUserName = userName,
        //                CommenterPhoto = userPic,
        //                Tags = item.Tags,
        //                UpdatedAt = item.UpdatedAt,
        //                Likes = item.Likes,
        //                Dislikes = item.Dislikes,
        //                UserIdsLikes = item.UserIdsLikes,
        //                UserIdsDislikes = item.UserIdsDislikes,
        //                Replies = item.Replies
        //            };

        //            coms.Add(com);
        //        }
        //        // TODO: REturn the Values you want by creating an new dto like PaginatedBlogResponseDto but for comments.
        //        return Ok(new { coms, result.To});
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while fetching all blogs.");
        //        return StatusCode(500, "An error occurred while fetching all blogs.");
        //    }
        //}
    }
}
