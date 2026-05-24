using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SuttorLibrary.Core;
using SuttorLibrary.Core.Services;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;
using System.Security.Claims;

namespace SuttorLibrary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(IUnitOfWork unit, ILogger<BooksController> logger, IFileService fileService, IConfiguration configuration) : ControllerBase
    {
        private readonly IUnitOfWork _unit = unit;
        private readonly ILogger<BooksController> _logger = logger;
        private readonly IFileService _fileService = fileService;
        private readonly IConfiguration _configuration = configuration;

        //GET /api/Books/{id}
        [HttpGet("{bookId}", Name = "GetBookById")]
        public async Task<IActionResult> GetBookById([FromRoute] string bookId)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(bookId))
                return BadRequest("Book ID is required.");

            try
            {
                var book = await _unit.BookRepo.GetBookWithDetailsAsync(bookId);
                if (book is null)
                    return NotFound($"Book with ID '{bookId}' not found.");

                Type type = book.GetType();

                string? photoPath = (string)type.GetProperty("photoPath")!.GetValue(book, null)!;
                IFormFile? bookCover = null;
                if (!string.IsNullOrEmpty(photoPath))
                {
                    bookCover = await _fileService.GetPictureAsync(photoPath);
                }

                var bookDto = new GetBookDTO
                {
                    Id = (Guid)type.GetProperty("ClientId")!.GetValue(book, null)!,
                    Title = (string)type.GetProperty("title")!.GetValue(book, null)!,
                    Description = (string)type.GetProperty("description")!.GetValue(book, null)!,
                    Photo = bookCover!,
                    FilePath = (string)type.GetProperty("filePath")!.GetValue(book, null)!,
                    PageCount = (int)type.GetProperty("pageCount")!.GetValue(book, null)!,
                    PublishedAT = (int)type.GetProperty("publishedAt")!.GetValue(book, null)!,
                    FileSize = (long)type.GetProperty("fileSize")!.GetValue(book, null)!,
                    UploadedAt = (DateTime)type.GetProperty("uploadedAt")!.GetValue(book, null)!,
                    language = (string)type.GetProperty("language")!.GetValue(book, null)!,
                    Authors_Names = (List<string>)type.GetProperty("authors")!.GetValue(book, null)!,
                    Categories_Names = (List<string>)type.GetProperty("categories")!.GetValue(book, null)!
                };

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book by ID {BookId}", bookId);
                return Problem("An error occurred while retrieving the book.");
            }
        }

        //GET /api/Books/ByName?name=...
        [HttpGet("ByName", Name = "GetBooksByName")]
        public async Task<IActionResult> GetBooksByName([FromQuery] string name)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(name))
                return BadRequest("Book name is required.");

            try
            {
                var book = await _unit.BookRepo.GetBookByName(name);
                if (book is null)
                    return NotFound($"No books found with the name '{name}'.");

                Type type = book.GetType();

                string? photoPath = (string)type.GetProperty("photoPath")!.GetValue(book, null)!;
                IFormFile? bookCover = null;
                if (!string.IsNullOrEmpty(photoPath))
                {
                    bookCover = await _fileService.GetPictureAsync(photoPath);
                }

                var bookDto = new GetBookDTO
                {
                    Id = (Guid)type.GetProperty("ClientId")!.GetValue(book, null)!,
                    Title = (string)type.GetProperty("title")!.GetValue(book, null)!,
                    Description = (string)type.GetProperty("description")!.GetValue(book, null)!,
                    Photo = bookCover!,
                    FilePath = (string)type.GetProperty("filePath")!.GetValue(book, null)!,
                    PageCount = (int)type.GetProperty("pageCount")!.GetValue(book, null)!,
                    PublishedAT = (int)type.GetProperty("publishedAt")!.GetValue(book, null)!,
                    FileSize = (long)type.GetProperty("fileSize")!.GetValue(book, null)!,
                    UploadedAt = (DateTime)type.GetProperty("uploadedAt")!.GetValue(book, null)!,
                    language = (string)type.GetProperty("language")!.GetValue(book, null)!,
                    Authors_Names = (List<string>)type.GetProperty("authors")!.GetValue(book, null)!,
                    Categories_Names = (List<string>)type.GetProperty("categories")!.GetValue(book, null)!
                };

                return Ok(bookDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by name {Name}", name);
                return Problem("An error occurred while retrieving the books.");
            }
        }

        //GET /api/Books/ByCategory?category=...
        [HttpGet("ByCategory", Name = "GetBooksByCategory")]
        public async Task<IActionResult> GetBooksByCategory([FromQuery] string category)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category name is required.");
            try
            {
                var books = await _unit.BookRepo.GetBooksByCategory(category);
                if (books is null || !books.Any())
                    return NotFound($"No books found in the category '{category}'.");

                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by category {Category}", category);
                return Problem("An error occurred while retrieving the books.");
            }
        }

        //GET /api/Books/ByAuthos?author?=...
        [HttpGet("ByAuthor")]
        public async Task<IActionResult> GetBooksByAuthor([FromQuery] string author)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(author))
                return BadRequest("Author name is required");

            try
            {
                var books = await _unit.BookRepo.GetBooksByAuthor(author);
                if (books is null || !books.Any())
                    return NotFound($"No books found in the author '{author}'.");

                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by author {Author}", author);
                return Problem("An error occurred while retrieving the books.");
            }
        }

        //Get /api/Books/authors
        [HttpGet("authors")]
        public async Task<IActionResult> GetAuthors([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be positive integers.");

            const int maxPageSize = 200;
            pageSize = Math.Min(pageSize, maxPageSize);

            try
            {
                var allAuthors = await _unit.BookRepo.GetAuthors();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLowerInvariant();
                    allAuthors = allAuthors?.Where(u =>
                        (u!.Name?.ToLowerInvariant().Contains(searchLower) ?? false)
                    ).ToList();
                }

                int total = allAuthors!.Count();
                var totalPages = (int)Math.Ceiling(total / (double)pageSize);
                var items = allAuthors!
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Admin requested authors page {Page}/{TotalPages} (size {PageSize}) search={Search}",
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
                _logger.LogError(ex, "Error retrieving all authors");
                return Problem("An error occurred while retrieving authors.");
            }
        }

        //Get /api/Books/categories?page=5&pageSize=10&search=
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be positive integers.");

            const int maxPageSize = 200;
            pageSize = Math.Min(pageSize, maxPageSize);

            try
            {
                var allCats = await _unit.BookRepo.GetCategories();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLowerInvariant();
                    allCats = allCats?.Where(u =>
                        (u!.Name?.ToLowerInvariant().Contains(searchLower) ?? false)
                    ).ToList();
                }

                var total = allCats!.Count();
                var totalPages = (int)Math.Ceiling(total / (double)pageSize);
                var items = allCats!
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Admin requested categories page {Page}/{TotalPages} (size {PageSize}) search={Search}",
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
                _logger.LogError(ex, "Error retrieving all categories");
                return Problem("An error occurred while retrieving categories.");
            }
        }

        //POST /api/Books/upload
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Author")]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadBookDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.File.Length == 0 || dto.File is null)
                return BadRequest("No file is added.");

            try
            {
                //Renaming the cover picture to the name of the file
                var coverName = dto.CoverPic.FileName;
                var coverExtension = Path.GetExtension(coverName);
                var coverFileName = $"{Path.GetFileNameWithoutExtension(dto.File.FileName)}_cover{(string.IsNullOrEmpty(coverExtension) ? string.Empty : coverExtension)}";

                var storagePath = _configuration["FileStorage:Path"] ??
                    throw new InvalidOperationException("FileStorage:Path not configured.");

                var filePath = Path.Combine(storagePath, dto.File.FileName);
                var photoPath = Path.Combine(storagePath, "Photos", coverFileName);

                // Get or create language
                var lang = (await _unit.BookRepo.GetLanguages())?
                    .FirstOrDefault(a => a!.Language == dto.language);
                if (lang is null)
                    await _unit.BookRepo.AddLanguage(dto.language);

                //Create a new book
                var book = new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = dto.File.FileName,
                    FilePath = filePath,
                    Description = dto.Description,
                    PageCount = dto.PageCount,
                    PhotoPath = photoPath,
                    FileSize = dto.File.Length,
                    FileType = dto.File.ContentType.ToLowerInvariant(),
                    PublishedAT = dto.PublishedAT,
                    UploadedAt = DateTime.UtcNow,
                    LanguageId = lang!.Id
                };
                //Add the book to the database
                await _unit.BookRepo.Add(book);

                //Filling the linking tables between Books, Authors, Categories, and languages.
                Category? cate;
                Author? auth;

                // Get or create category
                foreach (var catName in dto.Categories_Names)
                {
                    cate = (await _unit.BookRepo.GetCategories())?
                        .FirstOrDefault(c => c!.Name == catName);
                    if (cate is null)
                        await _unit.BookRepo.AddCategory(catName);

                    await _unit.BookRepo.LinkBookToCategory(book.Id, cate!.Id);
                }

                // Get or create author
                foreach (var authName in dto.Authors_Names)
                {
                    auth = (await _unit.BookRepo.GetAuthors())?
                        .FirstOrDefault(a => a!.Name == authName);
                    if (auth is null)
                        await _unit.BookRepo.AddAuthor(authName);

                    await _unit.BookRepo.LinkBookToAuthor(book.Id, auth!.Id);
                }

                //Save the file to the specified directory in appsettings.json
                var uploadedFile = await _fileService.UploadFileAsync(dto.File, dto.CoverPic);
                if (string.Equals(uploadedFile, "A file with the same name already exists.", StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"The file for '{dto.File.FileName}' already exists.");

                //Check if adding to the database succeeded. If not, delete the uploaded file and return an error.
                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    // Database commit failed — delete the uploaded file
                    _logger.LogError(dbEx, "Database commit failed. Deleting uploaded file: {FileName}", uploadedFile);
                    await _fileService.DeleteFileAsync(book.FilePath);
                    await _fileService.DeleteFileAsync(book.PhotoPath);
                    throw; // Re-throw to be caught by outer catch
                }

                _logger.LogInformation("Book uploaded successfully: {Title} with file {FileName}", dto.File.FileName, uploadedFile);

                return CreatedAtAction("GetBookById", new { bookId = book.Id }, new
                {
                    book.Id,
                    book.Title,
                    book.FilePath
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid file for book upload: {Title}", dto.File.FileName);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading book {Title}", dto.File.FileName);
                return Problem("An error occurred while uploading the book.");
            }
        }

        //GET /api/Books/{id}/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(id.ToString()))
                return BadRequest("No file to download");
            try
            {
                var book = await _unit.BookRepo.GetById(id.ToString());
                if (book is null || string.IsNullOrEmpty(book.FilePath))
                    return NotFound("This book is not found.");

                // Get current user id from Claims (may be null if anonymous)
                var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

                var fileStream = await _fileService.DownloadFileAsync(book.Title, book.FilePath, id, userId!);

                _logger.LogInformation("Book download initiated: {BookId} ({Title}) by user {UserId}", id, book.Title, userId ?? "anonymous");

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                // Database commit failed — delete the uploaded file
                _logger.LogError(dbEx, "Unable to add to Download table");
                    throw; // Re-throw to be caught by outer catch
                }

                return fileStream;
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, "Book file not found for book ID {BookId}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading book {BookId}", id);
                return Problem("An error occurred while downloading the book.");
            }
        }

        //Post /api/Books/addCategory
        [HttpPost("addCategory")]
        public async Task<IActionResult> AddBooksCategory([FromBody] string cat)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(cat))
                return BadRequest("The category name is required");

            try
            {
                var res = await _unit.BookRepo.AddCategory(cat);
                if (!res)
                    return BadRequest($"This category: {cat} already exists");

                return CreatedAtAction(nameof(AddBooksCategory), new { CategoryName = cat }, cat);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot add the category name: {Name}", cat);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the category name: {Name}", cat);
                return Problem("An error occurred while adding the category.");
            }
        }

        //Post /api/Books/addAuthor
        [HttpPost("addAuthor")]
        public async Task<IActionResult> AddBooksAuthor([FromBody] string author)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(author))
                return BadRequest("The author's name is required");

            try
            {
                var res = await _unit.BookRepo.AddAuthor(author, "");
                if (!res)
                    return BadRequest($"This author: {author} already exists");

                await _unit.CompleteAsync();

                return CreatedAtAction(nameof(AddBooksAuthor), new { AuthorName = author }, author);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot add the author: {Name}", author);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the author: {Name}", author);
                return Problem("An error occurred while adding the author.");
            }
        }

        //GET /api/Books/id/ratings
        [HttpGet("{id}/ratings")]
        public async Task<IActionResult> GetRatings([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id is null)
                return BadRequest("Book id is required");

            try { 
                var rates = await _unit.BookRepo.GetBookRatings(id);

                if (rates is null)
                    return NotFound("No rates were found");

                List<GetRatingDTO>? ratesDto = null;

                foreach (var rate in rates) 
                {
                    ratesDto!.Add(new GetRatingDTO{UserId = rate.UserId, Rating = rate.Rating, Comment = rate.Comment });
                }

                return Ok(ratesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the rating for book ID {BookId}", id);
                return Problem("An error occurred while adding the rating.");
            }
        }

        //Post /api/Books/id/AddRating
        [HttpPost("{id}/AddRating")]
        public async Task<IActionResult> AddBookRating([FromRoute] string id, [FromBody] RatingDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id is null || dto.UserId is null)
                return BadRequest();

            try
            {
                var res = await _unit.BookRepo.AddRating(id, dto);
                if (!res)
                    return BadRequest("Failed to add the rating.");
                await _unit.CompleteAsync();

                return CreatedAtAction(nameof(AddBookRating), new { dto.Rating, dto.Comment }, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the rating for book ID {BookId}", id);
                return Problem("An error occurred while adding the rating.");
            }
        }

        //Delete /api/Books/id/deleteRate?rateId=...
        [HttpDelete("{BookId}/deleteRate")]
        public async Task<IActionResult> DeleteBookRating([FromRoute] string BookId, [FromQuery] string rateId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (BookId is null || rateId is null)
                return BadRequest();
            try
            {
                var res = await _unit.BookRepo.DeleteRating(rateId);
                if (!res)
                    return BadRequest("Failed to delete the rating.");
                await _unit.CompleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot delete the rating with ID {RateId} for book ID {BookId}", rateId, BookId);
                return Problem("An error occurred while deleting the rating.");
            }
        }
    }
}
