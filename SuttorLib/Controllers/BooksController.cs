using LibrarySystem.Recommendations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuttorLib.Core.Interfaces;
using SuttorLib.Core.Services.Files;
using SuttorLib.Core.Services.Recommends.Mongo;
using SuttorLib.Models.Library;
using SuttorLibrary.Core;
using SuttorLibrary.DTOs;
using System.Net;
using System.Security.Claims;

namespace SuttorLib.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(
        IUnitOfWork unit, 
        ILogger<BooksController> logger, 
        IFileService fileService, 
        IConfiguration configuration, 
        IFCM fcm,
        IRecommendRepo repo,
        RecommendationPipelineService pipe
    ) : ControllerBase
    {
        private readonly IUnitOfWork _unit = unit;
        private readonly ILogger<BooksController> _logger = logger;
        private readonly IFileService _fileService = fileService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IFCM _fcm = fcm;
        private readonly IRecommendRepo _repository = repo;
        private readonly RecommendationPipelineService _pipeline = pipe;

        //Get /api/Books
        [HttpGet(Name = "GetAllBooks")]
        public async Task<IActionResult> GetAllBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be positive integers.");

            const int maxPageSize = 200;
            pageSize = Math.Min(pageSize, maxPageSize);

            try
            {
                var paged = await _unit.BookRepo.GetBooksPagedAsync(page, pageSize, search);

                if (paged.Total == 0)
                    return NotFound("No books found.");

                var items = new List<GetBookDTO>();
                foreach (var book in paged.Items)
                {
                    IFormFile? bookCover = null;
                    if (!string.IsNullOrEmpty(book.PhotoPath))
                    {
                        bookCover = await _fileService.GetPictureAsync(book.PhotoPath);
                    }

                    items.Add(new GetBookDTO
                    {
                        Id = book.Id,
                        Title = book.Title,
                        Description = book.Description,
                        PageCount = book.PageCount,
                        PublishedAT = book.PublishedAT,
                        FilePath = book.FilePath,
                        FileSize = book.FileSize / (1024 * 1024),
                        Photo = bookCover!,
                        UploadedAt = book.UploadedAt,
                        language = book.Language,
                        Authors_Names = book.Authors_Names,
                        Categories_Names = book.Categories_Names,
                        FileLink = BuildUrl(book.FilePath),
                        CoverLink = BuildUrl(book.PhotoPath)
                    });
                }

                _logger.LogInformation("The requested books page {Page}/{TotalPages} (size {PageSize}) search={Search}",
                    paged.Page, paged.TotalPages, paged.PageSize, search ?? "none");

                var result = new
                {
                    Total = (int) paged.Total,
                    Page = (int) paged.Page,
                    PageSize = (int) paged.PageSize,
                    TotalPages = (int) paged.TotalPages,
                    Items = items
                };

                return Ok(result);
            }
            catch (KeyNotFoundException nf)
            {
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all books");
                return Problem("An error occurred while retrieving books.");
            }
        }

        //GET /api/Books/{id}
        [HttpGet("{bookId}", Name = "GetBookById")]
        public async Task<IActionResult> GetBookById([FromRoute] string bookId, [FromBody] string? userId)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(bookId))
                return BadRequest("Book ID is required.");

            try
            {
                var book = await _unit.BookRepo.GetBookWithDetailsAsync(bookId, userId);
                if (book is null)
                    return NotFound($"Book with ID '{bookId}' not found.");

                Type type = book.GetType();

                string? photoPath = (string)type.GetProperty("photoPath")!.GetValue(book, null)!;
                IFormFile? bookCover = null;

                // extract authors names (authors is a list of anonymous objects { Name, Description })
                var authorsNames = new List<string>();
                var authorsObj = type.GetProperty("authors")?.GetValue(book, null);
                if (authorsObj is System.Collections.IEnumerable authorsEnum)
                {
                    foreach (var a in authorsEnum)
                    {
                        var aType = a?.GetType();
                        var nameVal = aType?.GetProperty("Name")?.GetValue(a, null)?.ToString();
                        if (!string.IsNullOrEmpty(nameVal))
                            authorsNames.Add(nameVal);
                    }
                }

                // extract categories names (categories is a list of anonymous objects { Name })
                var categoriesNames = new List<string>();
                var categoriesObj = type.GetProperty("categories")?.GetValue(book, null);
                if (categoriesObj is System.Collections.IEnumerable catsEnum)
                {
                    foreach (var c in catsEnum)
                    {
                        var cType = c?.GetType();
                        var nameVal = cType?.GetProperty("Name")?.GetValue(c, null)?.ToString();
                        if (!string.IsNullOrEmpty(nameVal))
                            categoriesNames.Add(nameVal);
                    }
                }

                if (!string.IsNullOrEmpty(photoPath))
                {
                    bookCover = await _fileService.GetPictureAsync(photoPath);
                }

                var encodedBookPath = BuildUrl((string)type.GetProperty("filePath")!.GetValue(book, null)!);
                var encodedPhotoPath = BuildUrl(photoPath);

                var bookDto = new GetBookDTO
                {
                    Id = (string)type.GetProperty("id")!.GetValue(book, null)!,
                    Title = (string)type.GetProperty("title")!.GetValue(book, null)!,
                    Description = (string)type.GetProperty("description")!.GetValue(book, null)!,
                    Photo = bookCover!,
                    FilePath = (string)type.GetProperty("filePath")!.GetValue(book, null)!,
                    PageCount = (int)type.GetProperty("pageCount")!.GetValue(book, null)!,
                    PublishedAT = (int)type.GetProperty("publishedAt")!.GetValue(book, null)!,
                    FileSize = (double)type.GetProperty("fileSize")!.GetValue(book, null)! / (1024 * 1024),
                    UploadedAt = (DateTime)type.GetProperty("uploadedAt")!.GetValue(book, null)!,
                    language = (string)type.GetProperty("language")!.GetValue(book, null)!,
                    Authors_Names = authorsNames,
                    Categories_Names = categoriesNames,
                    // Generate absolute URLs for the frontend
                    FileLink = encodedBookPath,
                    CoverLink = encodedPhotoPath,
                    IsFinished = (bool)type.GetProperty("isFinished")!.GetValue(book, null)!,
                    IsDownloaded = (bool)type.GetProperty("IsDownloaded")!.GetValue(book, null)!,
                    Ratings = (List<BookRating>)type.GetProperty("ratings")!.GetValue(book, null)!
                };

                return Ok(bookDto);
            }
            catch (KeyNotFoundException nf)
            {
                return NotFound(nf.Message);
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

                Type type = book.GetType();

                string? photoPath = (string)type.GetProperty("photoPath")!.GetValue(book, null)!;
                IFormFile? bookCover = null;

                // extract authors names (authors is a list of anonymous objects { Name, Description })
                var authorsNames = new List<string>();
                var authorsObj = type.GetProperty("authors")?.GetValue(book, null);
                if (authorsObj is System.Collections.IEnumerable authorsEnum)
                {
                    foreach (var a in authorsEnum)
                    {
                        var aType = a?.GetType();
                        var nameVal = aType?.GetProperty("Name")?.GetValue(a, null)?.ToString();
                        if (!string.IsNullOrEmpty(nameVal))
                            authorsNames.Add(nameVal);
                    }
                }

                // extract categories names (categories is a list of anonymous objects { Name })
                var categoriesNames = new List<string>();
                var categoriesObj = type.GetProperty("categories")?.GetValue(book, null);
                if (categoriesObj is System.Collections.IEnumerable catsEnum)
                {
                    foreach (var c in catsEnum)
                    {
                        var cType = c?.GetType();
                        var nameVal = cType?.GetProperty("Name")?.GetValue(c, null)?.ToString();
                        if (!string.IsNullOrEmpty(nameVal))
                            categoriesNames.Add(nameVal);
                    }
                }

                if (!string.IsNullOrEmpty(photoPath))
                {
                    bookCover = await _fileService.GetPictureAsync(photoPath);
                }

                var encodedBookPath = BuildUrl((string)type.GetProperty("filePath")!.GetValue(book, null)!);
                var encodedPhotoPath = BuildUrl(photoPath);

                var bookDto = new GetBookDTO
                {
                    Id = (string)type.GetProperty("id")!.GetValue(book, null)!,
                    Title = (string)type.GetProperty("title")!.GetValue(book, null)!,
                    Description = (string)type.GetProperty("description")!.GetValue(book, null)!,
                    Photo = bookCover!,
                    FilePath = (string)type.GetProperty("filePath")!.GetValue(book, null)!,
                    PageCount = (int)type.GetProperty("pageCount")!.GetValue(book, null)!,
                    PublishedAT = (int)type.GetProperty("publishedAt")!.GetValue(book, null)!,
                    FileSize = (double)type.GetProperty("fileSize")!.GetValue(book, null)! / (1024 * 1024),
                    UploadedAt = (DateTime)type.GetProperty("uploadedAt")!.GetValue(book, null)!,
                    language = (string)type.GetProperty("language")!.GetValue(book, null)!,
                    Authors_Names = authorsNames,
                    Categories_Names = categoriesNames,
                    // Generate absolute URLs for the frontend
                    FileLink = encodedBookPath,
                    CoverLink = encodedPhotoPath
                };

                return Ok(bookDto);
            }
            catch (KeyNotFoundException nf)
            {
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by name {Name}", name);
                return Problem("An error occurred while retrieving the books.");
            }
        }

        //GET /api/Books/ByCategory?category=...
        [HttpGet("ByCategory", Name = "GetBooksByCategory")]
        public async Task<IActionResult> GetBooksByCategory([FromQuery] string category, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(category))
                return BadRequest("Category name is required.");

            const int maxPageSize = 200;
            pageSize = Math.Min(pageSize, maxPageSize);

            try
            {
                var paged = await _unit.BookRepo.GetBooksByCategoryPagedAsync(category, page, pageSize);

                if (paged.Total == 0)
                    return NotFound($"No books found in category '{category}'.");

                var items = new List<GetBookDTO>();
                foreach (var book in paged.Items)
                {
                    items.Add(await BuildBookDtoAsync(book));
                }

                _logger.LogInformation("The requested books by category page {Page}/{TotalPages} (size {PageSize}) category={Category}",
                    paged.Page, paged.TotalPages, paged.PageSize, category);

                var result = new
                {
                    Total = (int)paged.Total,
                    Page = (int)paged.Page,
                    PageSize = (int)paged.PageSize,
                    TotalPages = (int)paged.TotalPages,
                    Items = items
                };

                return Ok(result);
            }
            catch (KeyNotFoundException nf)
            {
                return NotFound(nf.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving books by category {Category}", category);
                return Problem("An error occurred while retrieving the books.");
            }
        }

        //GET /api/Books/ByAuthos?author?=...
        [HttpGet("ByAuthor")]
        public async Task<IActionResult> GetBooksByAuthor([FromQuery] string author, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (string.IsNullOrEmpty(author))
                return BadRequest("Author name is required");

            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be positive integers.");

            const int maxPageSize = 200;
            pageSize = Math.Min(pageSize, maxPageSize);

            try
            {
                var paged = await _unit.BookRepo.GetBooksByAuthorPagedAsync(author, page, pageSize);

                if (paged.Total == 0)
                    return NotFound($"No books found for author '{author}'.");

                var items = new List<GetBookDTO>();
                foreach (var book in paged.Items)
                {
                    items.Add(await BuildBookDtoAsync(book));
                }

                _logger.LogInformation("The requested books by author page {Page}/{TotalPages} (size {PageSize}) author={Author}",
                    paged.Page, paged.TotalPages, paged.PageSize, author);

                var result = new
                {
                    Total =(int) paged.Total,
                    Page =(int) paged.Page,
                    PageSize =(int) paged.PageSize,
                    TotalPages =(int) paged.TotalPages,
                    Items = items
                };

                return Ok(result);
            }
            catch (KeyNotFoundException nf)
            {
                return NotFound(nf.Message);
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

                List<AuthorDTO?>? allAuthorsDto = [];

                foreach (var author in allAuthors!)
                {
                    var pic = author!.Picture is null ? null 
                        : await _fileService.GetPictureAsync(author!.Picture!);

                    allAuthorsDto.Add(new AuthorDTO { 
                        Id = author.Id,
                        Name = author.Name,
                        Picture = pic,
                        Description = author.Description,
                        IsRegistered = author.IsRegistered
                    });
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLowerInvariant();
                    allAuthorsDto = allAuthorsDto?.Where(u =>
                        (u!.Name?.ToLowerInvariant().Contains(searchLower) ?? false)
                    ).ToList();
                }

                int total = allAuthorsDto!.Count();
                var totalPages = (int)Math.Ceiling(total / (double)pageSize);
                var items = allAuthorsDto!
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
        [Authorize(Roles = "Admin,Author")]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(104857600)]
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

                var storagePath = _configuration["FileStorage:Path"];
                var filePath = Path.Combine(storagePath!, dto.File.FileName);
                var photoPath = Path.Combine(storagePath!, "Photos", coverFileName);

                // Get or create language
                var langList = await _unit.BookRepo.GetLanguages();
                var lang = langList!.FirstOrDefault(l => l!.Language == dto.language);

                //Create a new book
                var book = new Book
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = dto.File.FileName,
                    FilePath = filePath,
                    Description = dto.Description,
                    PageCount = dto.PageCount,
                    PhotoPath = photoPath,
                    Rating = 0f,
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
                    {
                        cate = await _unit.BookRepo.AddCategory(catName);
                        if (cate is null)
                            return BadRequest($"Category {cate!.Name} already exist or failed to create");
                    }
                    await _unit.BookRepo.LinkBookToCategory(book.Id, cate!.Id);
                }

                // Get or create author
                foreach (var authName in dto.Authors_Names)
                {
                    auth = (await _unit.BookRepo.GetAuthors())?
                        .FirstOrDefault(a => a!.Name == authName);
                    if (auth is null)
                    {
                        auth = await _unit.BookRepo.AddAuthor(Guid.NewGuid().ToString(), authName, "", false, "");
                        if(auth is null)
                            return BadRequest($"Author {auth!.Name} already exist or failed to create");
                    }

                    await _unit.BookRepo.LinkBookToAuthor(book.Id, auth!.Id);
                }

                //Save the file to the specified directory in appsettings.json
                var uploadedFile = await _fileService.UploadFileAsync(dto.File, dto.CoverPic);
                if (string.Equals(uploadedFile, "A file with the same name already exists.", StringComparison.OrdinalIgnoreCase))
                    return BadRequest($"The file for '{dto.File.FileName}' already exists.");

                if (uploadedFile is null)
                    _unit.BookRepo.Delete(book.Id);

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

                await _fcm.NotifyNewBookAsync(book.Title, dto.Categories_Names, dto.Authors_Names);

                var uid = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var user = await _unit.UserRepo.GetById(uid);
                await _unit.UserRepo.PromoteToAuthor(user!, 50);

                return CreatedAtAction("GetBookById", new { bookId = book.Id, XP = 50 }, new
                {
                        book.Id,
                        book.Title,
                        XP = 85,
                        message = "New XP Points added. Congrats!"
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
        [Authorize] // For preventing anonymous downloads
        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download([FromRoute] string id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(id))
                return BadRequest("No file to download");
            try
            {
                var book = await _unit.BookRepo.GetById(id.ToString());
                if (book is null || string.IsNullOrEmpty(book.FilePath))
                    return NotFound("This book is not found.");

                // Get current user id from Claims (null if anonymous)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

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

                var uid = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var user = await _unit.UserRepo.GetById(uid);
                if (user is not null)
                {
                    var isDownloaded = await _unit.UserRepo.GetUserDownloadHistory(user.Id, book.Id);
                    if (isDownloaded is not null)
                        await _unit.UserRepo.PromoteToAuthor(user, 50);
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddCategory([FromBody] AddCategory dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(dto.Name))
                return BadRequest("The category name is required");

            try
            {
                var res = await _unit.BookRepo.AddCategory(dto.Name);
                if (res is null)
                    return BadRequest($"This category: {dto.Name} already exists");

                var iconPath = dto.Icon is null ? null : await _fileService.UploadCategoryIcon(dto.Icon, res.Name);


                return CreatedAtAction(nameof(AddCategory), new { CategoryName = dto.Name }, dto.Name);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot add the category name: {Name}", dto.Name);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the category name: {Name}", dto.Name);
                return Problem("An error occurred while adding the category.");
            }
        }

        //Post /api/Books/addAuthor
        [HttpPost("addAuthor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddAuthor([FromBody] AddAuthorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(dto.Author))
                return BadRequest("The author's name is required");

            try
            {
                //Add The photo of the author
                string? PicPath = dto.Picture is null ? null : await _fileService.UploadUserPicAsync(dto.Picture, dto.Author, true);

                var res = await _unit.BookRepo.AddAuthor(Guid.NewGuid().ToString(), dto.Author, dto.Desc, false, PicPath);
                if (res is null)
                    return BadRequest($"This author: {dto.Author} already exists");

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to insert new author: {Author}", dto.Author);
                    throw; // Re-throw to be caught by outer catch
                }

                return CreatedAtAction(nameof(AddAuthor), new { AuthorName = dto.Author }, dto.Author);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot add the author: {Name}", dto.Author);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the author: {Name}", dto.Author);
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

                string username, fullName;
                IFormFile? photo = null;

                List<GetRatingDTO> ratesDto = new List<GetRatingDTO>();

                foreach (var rate in rates) 
                {
                    var user = await _unit.UserRepo.GetById(rate.UserId);
                    if (user is not null)
                    {
                        username = user.UserName;
                        fullName = user.FullName;
                    }
                    else {
                        username = "";  fullName = "Unknown User";
                        photo = user.PhotoPath is null ? null : await _fileService.GetPictureAsync(user.PhotoPath);
                    }
                    ratesDto.Add(
                        new GetRatingDTO
                        {
                            UserId = rate.UserId,
                            UserFullname = fullName,
                            UserPhoto = photo,
                            Username = username,
                            Rating = rate.Rating,
                            Comment = rate.Comment
                        }
                    );
                }

                return Ok(ratesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot add the rating for book ID {BookId}", id);
                return Problem("An error occurred while adding the rating.");
            }
        }

        //Post /api/Books/id/addRate
        [Authorize]
        [HttpPost("{id}/addRate")]
        public async Task<IActionResult> AddBookRating([FromRoute] string id, [FromBody] RatingDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id is null)
                return BadRequest("Blog Id is required.");

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var res = await _unit.BookRepo.AddRating(id, dto, userId);
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
        [Authorize]
        [HttpDelete("{BookId}/deleteRate")]
        public async Task<IActionResult> DeleteBookRating([FromRoute] string BookId, [FromQuery] string rateId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (BookId is null || rateId is null)
                return BadRequest();
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();
                var res = await _unit.BookRepo.DeleteRating(rateId, userId);
                if (!res)
                    return BadRequest("Failed to delete the rating.");

                await _unit.CompleteAsync();

                return NoContent();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot delete the rating with ID {RateId} for book ID {BookId}", rateId, BookId);
                return Problem("An error occurred while deleting the rating.");
            }
        }

        //Patch /api/Books/{id}/FinishRead
        [Authorize]
        [HttpPatch("{BookId}/finishReading")]
        public async Task<IActionResult> FinishBookReading([FromRoute] string BookId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (BookId is null || string.IsNullOrEmpty(BookId) || string.IsNullOrWhiteSpace(BookId))
                return BadRequest("The Book's ID is required");

            try {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var res = await _unit.BookRepo.IsFinishReading(BookId, userId);
                if (!res)
                    return BadRequest("Something went wrong. Please try again later.");

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to make IsFinished prop for the BOOK with the ID: {BookID}", BookId);
                    throw; // Re-throw to be caught by outer catch
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot read for book ID {BookId}", BookId);
                return Problem("An error occurred while reading.");
            }
        }

        // GET api/Books/{id}/readBook
        [HttpGet("{bookId}/read")]
        public async Task<IActionResult> ReadBook([FromRoute] string bookId)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            if (string.IsNullOrEmpty(bookId))
                return BadRequest("Book ID is required.");

            try {
                var book = await _unit.BookRepo.GetBookWithDetailsAsync(bookId, null);
                if (book is null)
                    return NotFound($"Book with ID '{bookId}' not found.");

                Type type = book.GetType();

                string? bookPath = (string)type.GetProperty("filePath")!.GetValue(book, null)!;
                string? bookType = (string)type.GetProperty("fileType")!.GetValue(book, null)!;

                //Make sure that the bookPath is in the root
                bookPath = _fileService.ResolveStoragePath(bookPath);

                if (!System.IO.File.Exists(bookPath))
                {
                    
                    _logger.LogWarning("File missing on disk for book {Id}: {Path}", bookId, bookPath);
                    return NotFound(new { message = "File could not be found on the server." });
                    
                }

                // enableRangeProcessing lets ASP.NET Core respond to HTTP byte-range
                // requests (206 Partial Content) — this is what lets PDF.js stream
                // the file page-by-page instead of downloading it all upfront.
                return PhysicalFile(bookPath, bookType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book by ID {BookId}", bookId);
                return Problem("An error occurred while retrieving the book.");
            }
        }

        // PATCH api/Books/authors/updateAuthor?authorId=...
        [Authorize(Roles = "Admin")]
        [HttpPatch("authors/updateAuthor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateAuthor([FromQuery] string authorId, [FromBody] UpdateAuthor dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (authorId is null)
                return BadRequest("Author Id is required");

            try {
                var authors = await _unit.BookRepo.GetAuthors();
                var author = authors!.FirstOrDefault(a => a!.Id == authorId);

                if (author == null)
                    return NotFound();

                var photoPath = dto.Picture is null ? null : await _fileService.UploadUserPicAsync(dto.Picture, author.Name, true);

                var result = await _unit.BookRepo.updateAuthor(authorId, author.Name, photoPath, dto.Description);

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to update author: {Name}", author.Name);
                    throw; // Re-throw to be caught by outer catch
                }

                return Ok($"Author {result.Name} has been updated");
             }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update the author with id {ID}", authorId);
                return Problem("An error occurred while updating author.");
            }
        }

        // PATCH api/Books/authors/updateCategory?categoryId=...
        [Authorize(Roles = "Admin")]
        [HttpPatch("authors/updateCategory")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCategory([FromQuery] string categoryId, [FromBody] IFormFile? icon)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoryId is null)
                return BadRequest("Category Id is required");
            
            try {
                var cats = await _unit.BookRepo.GetCategories();
                var cat = cats!.FirstOrDefault(c => c!.Id == categoryId);

                if (cat is null)
                    return NotFound();

                var iconPath = icon is null ? null : await _fileService.UploadCategoryIcon(icon, cat.Name);

                var res = await _unit.BookRepo.updateCategory(categoryId, cat.Name, iconPath);

                try
                {
                    await _unit.CompleteAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Failed to update the category {Name}", cat);
                    throw; // Re-throw to be caught by outer catch
                }

                return Ok($"Category {res.Name} has been updated.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot update the category with id {ID}", categoryId);
                return Problem("An error occurred while updating category.");
            }
        }

        // GET api/Books/Recommendations
        [Authorize]
        [HttpGet("Recommendations")]
        public async Task<IActionResult> GetForUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId is null)
                return Unauthorized();

            var doc = await _repository.GetForUserAsync(userId);
            if (doc is null)
                return NotFound(new { message = "No recommendations generated yet for this user." });

            return Ok(doc);
        }

        //GET api/Books/train
        [Authorize(Roles = "Admin")]
        [HttpPost("train")]
        public async Task<IActionResult> RetrainAndGenerate()
        {
            await _pipeline.RunAsync();
            return Ok(new { message = "Recommendations regenerated." });
        }

        // POST api/Books/{id}/SetBookAsFavorite
        [Authorize]
        [HttpPost("{bookId}/SetBookAsFavorite")]
        public async Task<IActionResult> SetFavoriteBook([FromRoute] string bookId)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(bookId))
                return BadRequest("Book Id is required");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            try
            {

                var res = await _unit.BookRepo.AddFavoriteBook(bookId, userId);
                if (!res) 
                {
                    _logger.LogInformation("Cannot adding The Book {book} to favorite list for the user {user}", bookId, userId);
                    return BadRequest("Something went wrong, Please try again!");
                }

                _logger.LogInformation("Successfully adding The Book {book} to favorite list for the user {user}", bookId, userId);
                return Created();
            }
            catch (InvalidOperationException io) 
            {
                _logger.LogError("Invalid Operation happend in SetFavoriteBook");
                return BadRequest(io);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot set the favorite book for the user with id {ID}", userId);
                return Problem("An error occurred while setting favorite book.");
            }
        }

        //DELETE api/Books/RemoveFromFavorite
        [Authorize]
        [HttpDelete("{bookId}/RemoveBookFromFavorites")]
        public async Task<IActionResult> RemoveBookFromFavorite([FromRoute] string bookId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrEmpty(bookId))
                return BadRequest("Book Id is required");

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try {
                var res = await _unit.BookRepo.RemoveFavoriteBook(bookId, userId);
                if (!res)
                {
                    _logger.LogInformation("Cannot remove The Book {book} to favorite list for the user {user}", bookId, userId);
                    return BadRequest("Something went wrong, Please try again!");
                }

                _logger.LogInformation("Successfully removing The Book {book} to favorite list for the user {user}", bookId, userId);
                return Ok();
            }
            catch (KeyNotFoundException nf)
            {
                _logger.LogError("Invalid Operation happend in SetFavoriteBook");
                return NotFound(nf);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot set the favorite book for the user with id {ID}", userId);
                return Problem("An error occurred while setting favorite book.");
            }
        }

        //GET api/Books/Favorites
        [Authorize]
        [HttpGet("FavoriteBooks")]
        public async Task<IActionResult> GetFavorites()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try {
                var result = await _unit.BookRepo.GetFavoriteBooks(userId);
                if (result is null || result.Count() == 0)
                    return NotFound("No books in your favorites list");

                _logger.LogInformation("Successfully getting books for user {id}", userId);
                return Ok(result.ToList());
            }
            catch (KeyNotFoundException nf)
            {
                _logger.LogError("Invalid Operation happend in SetFavoriteBook");
                return NotFound(nf);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot set the favorite book for the user with id {ID}", userId);
                return Problem("An error occurred while setting favorite book.");
            }
        }

        private string BuildUrl(string relativePath)
        {

            var baseUrl = _configuration["FileStorage:ApiBaseUrl"] ?? "https://suttor.runasp.net";
            return $"{baseUrl.TrimEnd('/')}/{ToURLPath(relativePath)}";
        }

        private static string ToURLPath(string relativePath)
        {
            var segments = relativePath.Replace('\\', '/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            return string.Join("/", segments.Select(Uri.EscapeDataString));
        }

        // Shared helper that maps a BookListItemDto to the public GetBookDTO and loads the cover photo
        private async Task<GetBookDTO> BuildBookDtoAsync(BookListItemDto book)
        {
            IFormFile? bookCover = null;
            if (!string.IsNullOrEmpty(book.PhotoPath))
            {
                bookCover = await _fileService.GetPictureAsync(book.PhotoPath);
            }

            return new GetBookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                PageCount = book.PageCount,
                PublishedAT = book.PublishedAT,
                FilePath = book.FilePath,
                FileSize = book.FileSize / (1024 * 1024),
                Photo = bookCover!,
                UploadedAt = book.UploadedAt,
                language = book.Language,
                Authors_Names = book.Authors_Names,
                Categories_Names = book.Categories_Names,
                FileLink = BuildUrl(book.FilePath),
                CoverLink = BuildUrl(book.PhotoPath)
            };
        }
    }
}
