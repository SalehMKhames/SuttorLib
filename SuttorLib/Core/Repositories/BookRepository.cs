using Microsoft.EntityFrameworkCore;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.Models;

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
                    language = _context.BookLanguages
                        .Where(bl => bl.BookId == b.Id)
                        .Join(
                            _context.Languages,
                            bl => bl.LanguageId,
                            l => l.Id,
                            (bl, l) => new { l.Id, l.Language }
                        )
                        .FirstOrDefault()
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
                    language = _context.BookLanguages
                        .Where(bl => bl.BookId == b.Id)
                        .Join(
                            _context.Languages,
                            bl => bl.LanguageId,
                            l => l.Id,
                            (bl, l) => new { l.Id, l.Language }
                        )
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
                                (ba2, a2) => new { a2.Id, a2.Name, a2.Description }
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
                        language = _context.BookLanguages
                            .Where(bl => bl.BookId == b.Id)
                            .Join(
                                _context.Languages,
                                bl => bl.LanguageId,
                                l => l.Id,
                                (bl, l) => new { l.Id, l.Language }
                            )
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
                                (ba, a) => new { a.Id, a.Name, a.Description }
                            )
                            .ToList(),
                        categories = _context.BookCategories
                            .Where(bc2 => bc2.bookId == b.Id)
                            .Join(
                                _context.Categories,
                                bc2 => bc2.categoryId,
                                c2 => c2.Id,
                                (bc2, c2) => new { c2.Id, c2.Name }
                            )
                            .ToList(),
                        language = _context.BookLanguages
                            .Where(bl => bl.BookId == b.Id)
                            .Join(
                                _context.Languages,
                                bl => bl.LanguageId,
                                l => l.Id,
                                (bl, l) => new { l.Id, l.Language }
                            )
                            .FirstOrDefault()
                    }
                ))
            .ToListAsync();

            return books.Count > 0 ? books : null;
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

        public async Task<bool> AddCategory(string category)
        {
            var result = await _context.Categories.AnyAsync(c => c.Name.ToLower() == category.ToLower());
            if (result)
                return false;

            var cat = new Category { Id = Guid.NewGuid().ToString(), Name = category };

            await _context.Categories.AddAsync(cat);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddAuthor(string author, string desc = "")
        {
            var result = await _context.Authors.AnyAsync(a => a.Name == author);
            if (result)
                return false;

            var auth = new Author
            {
                Id = Guid.NewGuid().ToString(),
                Name = author,
                Description = desc ?? ""
            };

            await _context.Authors.AddAsync(auth);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddLanguage(string langName)
        {
            var res = await _context.Languages.AnyAsync(l => l.Language == langName);
            if(res)
                return false;

            var lang = new Languages 
            { 
                Id = Guid.NewGuid().ToString(),
                Language = langName
            };

            await _context.Languages.AddAsync(lang);
            await _context.SaveChangesAsync();
            return true;
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

        public async Task LinkBookToLanguage(string bookId, string langId)
        {
            var bookLanguage = new BookLanguages
            {
                
                BookId = bookId,
                LanguageId = langId
            };
            await _context.BookLanguages.AddAsync(bookLanguage);
            await _context.SaveChangesAsync();
        }
    }
}
