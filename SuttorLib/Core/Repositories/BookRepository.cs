using Microsoft.EntityFrameworkCore;
using SuttorLib.Models.Library;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.DTOs;
using System.Linq.Expressions;

namespace SuttorLibrary.Core.Repositories
{
    public class BookRepository(AppDbContext context) : GenericRepo<Book>(context), IBookRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<object?> GetBookWithDetailsAsync(string id, string? userId)
        {
            if (!Guid.TryParse(id, out Guid guidId))
                return null;

            var result = await _context.Books
                .Where(b => b.Id == guidId.ToString())
                .Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    description = b.Description,
                    pageCount = b.PageCount,
                    publishedAt = b.PublishedAT,
                    fileType = b.FileType,
                    uploadedAt = b.UploadedAt,
                    fileSize = b.FileSize,
                    filePath = b.FilePath,
                    photoPath = b.PhotoPath,
                    authors = _context.BookAuthors
                        .Where(ba => ba.Book_Id == b.Id)
                        .Join(
                            _context.Authors,
                            ba => ba.Author_Id,
                            a => a.Id,
                            (ba, a) => new { a.Id, a.Name, a.Description }
                        )
                        .ToList(),
                    categories = _context.BookCategories
                        .Where(bc => bc.bookId == b.Id)
                        .Join(
                            _context.Categories,
                            bc => bc.categoryId,
                            c => c.Id,
                            (bc, c) => new { c.Id, c.Name }
                        )
                        .ToList(),
                    language = _context.Languages
                        .Where(l => l.Id == b.LanguageId)
                        .Select(l => l.Language)
                        .FirstOrDefault(),
                    downloads = _context.Downloads
                        .Count(d => d.BookID == b.Id),

                    IsDownloaded = string.IsNullOrEmpty(userId)
                        ? false
                        : _context.Downloads.Any(d => d.BookID == b.Id && d.UserID == userId),

                    ratings = _context.BookRatings
                        .Where(r => r.BookId == b.Id)
                        .ToList(),

                    isFinished =
                    !string.IsNullOrEmpty(userId) && _context.Downloads
                        .Where(d => d.BookID == b.Id && d.UserID == userId)
                        .Select(d => d.IsFinishReading)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (result is null)
                throw new KeyNotFoundException("Book not found");

            return result;
        }

        public override Task Add(Book entity)
        {
            return base.Add(entity);
        }

        public override void Update(Book entity)
        {
            base.Update(entity);
        }

        public async Task<BookListItemDto?> GetBookByNameAsync(string bookName)
        {
            var book = await _context.Books
                .AsNoTracking()
                .Where(b => b.Title.ToLower() == bookName.ToLower())
                .Select(BookListProjection)
                .FirstOrDefaultAsync();

            return book ?? throw new KeyNotFoundException($"No book found matching '{bookName}'.");
        }

        public async Task<bool> AddRating(string BookId, RatingDTO dto, string userId)
        {
            var existingRating = await _context.BookRatings.AddAsync(new BookRating
            {
                Id = Guid.NewGuid().ToString(),
                BookId = BookId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            });

            //update authors' rating
            await UpdateBookRating(BookId);

            return existingRating != null;
        }

        public async Task<bool> DeleteRating(string rateId, string userId)
        {
            var rate = await _context.BookRatings.FirstOrDefaultAsync(r => r.Id == rateId);
            if (rate == null) 
                throw new KeyNotFoundException();

            if (userId != rate.UserId)
                throw new UnauthorizedAccessException();

            _context.Remove(rate);

            //update authors' rating
            await UpdateBookRating(rate.BookId);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<BookRating>?> GetBookRatings(string BookId) 
        {
            if (!Guid.TryParse(BookId, out Guid guidId))
                return null;

            var bookRatings = await _context.BookRatings
                .Where(br => br.BookId == BookId)
                .ToListAsync();

            return bookRatings.Count > 0 ? bookRatings : null;
        }

        public async Task<IEnumerable<Category?>?> GetCategories()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<IEnumerable<Author?>?> GetAuthors()
        {
            return await _context.Authors.ToListAsync();
        }

        public async Task<IEnumerable<Languages?>?> GetLanguages()
        {
            return await _context.Languages.ToListAsync();
        }

        public async Task<Category?> AddCategory(string category)
        {
            var result = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.ToLower());
            if (!result)
            {
                var cat = new Category { Id = Guid.NewGuid().ToString(), Name = category };

                await _context.Categories.AddAsync(cat);
                await _context.SaveChangesAsync();

                return cat;
            }

            return null;
        }

        public async Task<Author?> AddAuthor(string Id, string author, string desc = "", bool IsReg = false, string? Photo = "")
        {
            var result = await _context.Authors.AnyAsync(a => a.Name == author);
            if (!result)
            {
                var auth = new Author
                {
                    Id = Id,
                    Name = author,
                    Description = desc,
                    IsRegistered = IsReg,
                    Picture = Photo
                };

                await _context.Authors.AddAsync(auth);
                await _context.SaveChangesAsync();

                return auth;
            }

            return null;
        }

        public async Task<Languages?> AddLanguage(string langName)
        {
            var res = await _context.Languages.AnyAsync(l => l.Language == langName);
            if (!res)
            {
                var lang = new Languages
                {
                    Id = Guid.NewGuid().ToString(),
                    Language = langName
                };

                await _context.Languages.AddAsync(lang);
                await _context.SaveChangesAsync();
                return lang;
            }

            return null;
        }

        public async Task LinkBookToAuthor(string bookId, string authorId)
        {
            var bookAuthor = new BookAuthors
            {
                Id = Guid.NewGuid().ToString(),
                Book_Id = bookId,
                Author_Id = authorId
            };

            await _context.BookAuthors.AddAsync(bookAuthor);
            await _context.SaveChangesAsync();
        }

        public async Task LinkBookToCategory(string bookId, string categoryId)
        {
            var bookCategory = new BookCategories
            {
                Id = Guid.NewGuid().ToString(),
                bookId = bookId,
                categoryId = categoryId
            };

            await _context.BookCategories.AddAsync(bookCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsFinishReading(string bookId, string userId)
        {
            var download = await _context.Downloads
                .FirstOrDefaultAsync(d => d.BookID == bookId && d.UserID == userId);

            if (download is null)
                throw new InvalidOperationException("You must download the book before marking it as finished.");


            download.IsFinishReading = true;
            _context.Update(download);

            return true;
        }

        public async Task<Category?> updateCategory(string catId, string catName, string? catIcon)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == catId);
            if (category is null)
                throw new KeyNotFoundException(catName);

            category.Name = catName;
            category.Icon = catIcon is null ? category.Icon : catIcon;

            _context.Update(category);
            await _context.SaveChangesAsync();
            
            return category;
        }

        public async Task<Author?> updateAuthor(string authId, string authName, string? authPhoto, string authDesc)
        {
            var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == authId);
            if (author is null)
                throw new KeyNotFoundException(authName);

            author.Picture = authPhoto is null ? author.Picture : authPhoto;
            author.Description = authDesc;

            _context.Update(author);
            await _context.SaveChangesAsync();

            return author;
        }
        
        private async Task UpdateBookRating(string id)
        {
            if (id is null)
                throw new ArgumentException();

            var bookRatings = await _context.BookRatings
                .Where(br => br.BookId == id)
                .Select(br => br.Rating)
                .ToListAsync();

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book is null)
                throw new KeyNotFoundException("Book not found");

            book.Rating = bookRatings.Count > 0 ? bookRatings.Average() : 0f;

            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<BookListItemDto>> GetBooksPagedAsync(int page, int pageSize, string? search)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(pageSize, 200));

            IQueryable<Book> query = _context.Books.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLowerInvariant();
                query = query.Where(b => b.Title.ToLower().Contains(searchLower));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookListItemDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    PageCount = b.PageCount,
                    PublishedAT = b.PublishedAT,
                    FilePath = b.FilePath,
                    FileSize = b.FileSize,
                    PhotoPath = b.PhotoPath,
                    UploadedAt = b.UploadedAt,
                    Language = _context.Languages
                        .Where(l => l.Id == b.LanguageId)
                        .Select(l => l.Language)
                        .FirstOrDefault() ?? string.Empty,
                    Authors_Names = _context.BookAuthors
                        .Where(ba => ba.Book_Id == b.Id)
                        .Join(
                            _context.Authors,
                            ba => ba.Author_Id,
                            a => a.Id,
                            (ba, a) => a.Name)
                        .ToList(),
                    Categories_Names = _context.BookCategories
                        .Where(bc => bc.bookId == b.Id)
                        .Join(
                            _context.Categories,
                            bc => bc.categoryId,
                            c => c.Id,
                            (bc, c) => c.Name)
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<BookListItemDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<BookListItemDto>> GetBooksByCategoryPagedAsync(string categoryName, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(pageSize, 200));

            var query = _context.Categories
                .Where(c => c.Name.ToLower() == categoryName.ToLower())
                .Join(
                    _context.BookCategories,
                    c => c.Id,
                    bc => bc.categoryId,
                    (c, bc) => bc)
                .Join(
                    _context.Books,
                    bc => bc.bookId,
                    b => b.Id,
                    (bc, b) => b)
                .Distinct()
                .AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(BookListProjection)
                .ToListAsync();

            return new PagedResult<BookListItemDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<PagedResult<BookListItemDto>> GetBooksByAuthorPagedAsync(string authorName, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, Math.Min(pageSize, 200));

            var query = _context.Authors
                .Where(a => a.Name.ToLower() == authorName.ToLower())
                .Join(
                    _context.BookAuthors,
                    a => a.Id,
                    ba => ba.Author_Id,
                    (a, ba) => ba)
                .Join(
                    _context.Books,
                    ba => ba.Book_Id,
                    b => b.Id,
                    (ba, b) => b)
                .Distinct()
                .AsNoTracking();

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(BookListProjection)
                .ToListAsync();

            return new PagedResult<BookListItemDto>
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            };
        }

        public async Task<bool> AddFavoriteBook(string bookId, string userId)
        {
            var existingFavorite = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.BookId == bookId && fb.UserId == userId);
            if (existingFavorite is not null)
                throw new InvalidOperationException("This book is already in your favorites.");

            var favoriteBook = new FavoriteBooks
            { 
                Id = Guid.NewGuid().ToString(),
                BookId = bookId,
                UserId = userId
            };

            await _context.FavoriteBooks.AddAsync(favoriteBook);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFavoriteBook(string bookId, string userId)
        {
            var existingFavorite = await _context.FavoriteBooks
                .FirstOrDefaultAsync(fb => fb.BookId == bookId && fb.UserId == userId);
            if (existingFavorite is null)
                throw new KeyNotFoundException("This book is not in your favorites.");

            _context.FavoriteBooks.Remove(existingFavorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>?> GetFavoriteBooks(string userId)
        {
            var books = await _context.FavoriteBooks
                .Where(fb => fb.UserId == userId)
                .Join(
                    _context.Books,
                    fb => fb.BookId,
                    b => b.Id,
                    (fb, b) => b
                )
                .Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    description = b.Description,
                    pageCount = b.PageCount,
                    publishedAt = b.PublishedAT,
                    fileType = b.FileType,
                    uploadedAt = b.UploadedAt,
                    fileSize = b.FileSize,
                    filePath = b.FilePath,
                    photoPath = b.PhotoPath,
                    authors = _context.BookAuthors
                        .Where(ba => ba.Book_Id == b.Id)
                        .Join(
                            _context.Authors,
                            ba => ba.Author_Id,
                            a => a.Id,
                            (ba, a) => new { a.Id, a.Name, a.Description }
                        )
                        .ToList(),
                    categories = _context.BookCategories
                        .Where(bc => bc.bookId == b.Id)
                        .Join(
                            _context.Categories,
                            bc => bc.categoryId,
                            c => c.Id,
                            (bc, c) => new { c.Id, c.Name }
                        )
                        .ToList(),
                    language = _context.Languages
                        .Where(l => l.Id == b.LanguageId)
                        .Select(l => l.Language)
                        .FirstOrDefault(),
                    downloads = _context.Downloads.Count(d => d.BookID == b.Id),
                    IsDownloaded = _context.Downloads.Any(d => d.BookID == b.Id && d.UserID == userId),
                    ratings = _context.BookRatings.Where(r => r.BookId == b.Id).ToList(),
                    isFinished = _context.Downloads
                        .Where(d => d.BookID == b.Id && d.UserID == userId)
                        .Select(d => d.IsFinishReading)
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (books.Count == 0)
                throw new KeyNotFoundException("There is no book in your favorites");

            return books;
        }

        private Expression<Func<Book, BookListItemDto>> BookListProjection => b => new BookListItemDto
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            PageCount = b.PageCount,
            PublishedAT = b.PublishedAT,
            FilePath = b.FilePath,
            FileSize = b.FileSize,
            PhotoPath = b.PhotoPath,
            UploadedAt = b.UploadedAt,
            Language = _context.Languages
                .Where(l => l.Id == b.LanguageId)
                .Select(l => l.Language)
                .FirstOrDefault() ?? string.Empty,
            Authors_Names = _context.BookAuthors
                .Where(ba => ba.Book_Id == b.Id)
                .Join(
                    _context.Authors,
                    ba => ba.Author_Id,
                    a => a.Id,
                    (ba, a) => a.Name)
                .ToList(),
            Categories_Names = _context.BookCategories
                .Where(bc => bc.bookId == b.Id)
                .Join(
                    _context.Categories,
                    bc => bc.categoryId,
                    c => c.Id,
                    (bc, c) => c.Name)
                .ToList()
        };
    }
}
