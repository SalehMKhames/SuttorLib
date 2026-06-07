using Microsoft.EntityFrameworkCore;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;
using System.Net;

namespace SuttorLibrary.Core.Repositories
{
    public class BookRepository(AppDbContext context) : GenericRepo<Book>(context), IBookRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<object?> GetBookWithDetailsAsync(string id)
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
                            (ba, a) => new { a.Name, a.Description }
                        )
                        .ToList(),
                    categories = _context.BookCategories
                        .Where(bc => bc.bookId == b.Id)
                        .Join(
                            _context.Categories,
                            bc => bc.categoryId,
                            c => c.Id,
                            (bc, c) => new { c.Name }
                        )
                        .ToList(),
                    language = _context.Languages
                        .Where(l => l.Id == b.LanguageId)
                        .Select(l => l.Language )
                        .FirstOrDefault(),
                    downloads = _context.Downloads
                        .Where(d => d.BookID == b.Id)
                        .Join(
                            _context.AppUsers,
                            d => d.BookID,
                            u => u.Id,
                            (d, u) => new { d.Id }
                        ).Count()
                })
                .FirstOrDefaultAsync();

            if (result is null)
                return null;

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

        public async Task<object?> GetBookByName(string bookName)
        {
            var book = await _context.Books.Where(b => b.Title == bookName)
                .Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    description = b.Description,
                    pageCount = b.PageCount,
                    publishedAt = b.PublishedAT,
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
                            (ba, a) => new { a.Name, a.Description }
                        )
                        .ToList(),
                    categories = _context.BookCategories
                        .Where(bc => bc.bookId == b.Id)
                        .Join(
                            _context.Categories,
                            bc => bc.categoryId,
                            c => c.Id,
                            (bc, c) => new { c.Name }
                        )
                        .ToList(),
                    language = _context.Languages
                        .Where(l => l.Id == b.LanguageId)
                        .Select(l => l.Language )
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();
            
            if (book is null)
                return null;

            return book;
        }

        public async Task<IEnumerable<object?>?> GetBooksByAuthor(string authorName)
        {
            var books = await _context.Authors
                .Where(a => a.Name.ToLower() == authorName.ToLower())
                .SelectMany(a => _context.BookAuthors
                .Where(ba => ba.Author_Id == a.Id)
                .Join(
                    _context.Books,
                    ba => ba.Book_Id,
                    b => b.Id,
                    (ba, b) => new
                    {
                        id = b.Id,
                        title = b.Title,
                        description = b.Description,
                        pageCount = b.PageCount,
                        publishedAt = b.PublishedAT,
                        uploadedAt = b.UploadedAt,
                        fileSize = b.FileSize,
                        filePath = b.FilePath,
                        photoPath = b.PhotoPath,
                        authors = _context.BookAuthors
                            .Where(ba2 => ba2.Book_Id == b.Id)
                            .Join(
                                _context.Authors,
                                ba2 => ba2.Author_Id,
                                a2 => a2.Id,
                                (ba2, a2) => new {  a2.Name, a2.Description }
                            )
                        .ToList(),
                        categories = _context.BookCategories
                            .Where(bc => bc.bookId == b.Id)
                            .Join(
                                _context.Categories,
                                bc => bc.categoryId,
                                c => c.Id,
                                (bc, c) => new { c.Name }
                            )
                            .ToList(),
                        language = _context.Languages
                            .Where(l => l.Id == b.Id)
                            .Select(l => l.Language)
                            .FirstOrDefault()
                    }
                ))
            .ToListAsync();

            return books.Count > 0 ? books : null;
        }

        public async Task<IEnumerable<object?>?> GetBooksByCategory(string categoryName)
        {
            var books = await _context.Categories
            .Where(c => c.Name.ToLower() == categoryName.ToLower())
            .SelectMany(c => _context.BookCategories
                .Where(bc => bc.categoryId == c.Id)
                .Join(
                    _context.Books,
                    bc => bc.bookId,
                    b => b.Id,
                    (bc, b) => new
                    {
                        id = b.Id,
                        title = b.Title,
                        description = b.Description,
                        pageCount = b.PageCount,
                        publishedAt = b.PublishedAT,
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
                                (ba, a) => new { a.Name, a.Description }
                            )
                            .ToList(),
                        categories = _context.BookCategories
                            .Where(bc2 => bc2.bookId == b.Id)
                            .Join(
                                _context.Categories,
                                bc2 => bc2.categoryId,
                                c2 => c2.Id,
                                (bc2, c2) => new { c2.Name }
                            )
                            .ToList(),
                        language = _context.Languages
                            .Where(l => l.Id == b.Id)
                            .Select(l => l.Language)
                            .FirstOrDefault()
                    }
                ))
            .ToListAsync();

            return books.Count > 0 ? books : null;
        }

        public async Task<bool> AddRating(string BookId, RatingDTO dto)
        { 
            var existingRating = await _context.BookRatings.AddAsync(new BookRating
            {
                Id = Guid.NewGuid().ToString(),
                BookId = BookId,
                UserId = dto.UserId,
                Rating = dto.Rating,
                Comment = dto.Comment
            });

            //update authors' rating
            await UpdateAuthorRating(BookId);

            await _context.SaveChangesAsync();

            return existingRating != null;
        }

        public async Task<bool> DeleteRating(string rateId)
        {
            var rate = await _context.BookRatings.FirstOrDefaultAsync(r => r.Id == rateId);
            if (rate == null) 
                return false;

            _context.Remove(rate);

            //update authors' rating
            await UpdateAuthorRating(rate.BookId);

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
                    Description = desc ?? "",
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

        private async Task UpdateAuthorRating(string bookID)
        {
            var authorsIDs = await _context.BookAuthors
                .Where(ba => ba.Book_Id == bookID)
                .Select(ba => ba.Author_Id)
                .Distinct()
                .ToListAsync();

            if(authorsIDs is null || authorsIDs.Count == 0)
                return;

            foreach (var authorId in authorsIDs)
            {
                // all book ids for this author
                var authorBookIds = await _context.BookAuthors
                    .Where(ba => ba.Author_Id == authorId)
                    .Select(ba => ba.Book_Id)
                    .Distinct()
                    .ToListAsync();

                if (authorBookIds == null || authorBookIds.Count == 0)
                {
                    // set rating to 0 if author has no books
                    var authorEmpty = await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
                    if (authorEmpty != null)
                        authorEmpty.Rating = 0f;
                    continue;
                }

                // compute average rating across the author's books
                var ratingsQuery = _context.BookRatings
                    .Where(br => authorBookIds.Contains(br.BookId))
                    .Select(br => br.Rating);

                float avgRating = 0f;

                // If there are no ratings, default to 0
                var anyRatings = await ratingsQuery.AnyAsync();
                if (anyRatings)
                    avgRating = await ratingsQuery.AverageAsync();

                var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId);
                if (author != null)
                    author.Rating = avgRating;
            }

            await _context.SaveChangesAsync();
        }
    }
}
