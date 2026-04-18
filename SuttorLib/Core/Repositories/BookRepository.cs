using Microsoft.EntityFrameworkCore;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Repositories
{
    public class BookRepository(AppDbContext context) : GenericRepo<Book>(context), IBookRepository
    {
        private readonly AppDbContext _context = context;

        public Task<IQueryable> GetByIdFromQuery(string id)
        {
            var result = from b in _context.Books
                         join ba in _context.BookAuthors
                         on b.Id equals ba.Book_Id
                         join a in _context.Authors
                         on ba.Author_Id equals a.Id
                         join bc in _context.BookCategories
                         on b.Id equals bc.bookId
                         join c in _context.Categories
                         on bc.categoryId equals c.Id
                         select new
                         {
                             b.Id,
                             b.Title,
                             b.Description,
                             b.PageCount,
                             b.FileSize,
                             b.PublishedAT,
                             b.UploadedAt,
                             b.FilePath,
                             b.PhotoPath,
                             category = c.Name,
                             author = a.Name
                         };

            return (Task<IQueryable>)result;
        }

        public override Task Add(Book entity)
        {
            return base.Add(entity);
        }

        public override void Update(Book entity)
        {
            base.Update(entity);
        }

        public async Task<Book?> GetBookByName(string bookName)
        {
            var book = await _context.Books.FindAsync(bookName);
            if (book is null)
                return null;

            return book;
        }

        public async Task<IEnumerable<Book?>?> GetBooksByAuthor(string authorName)
        {
            var books = await _context.BookAuthors
                .Where(ba => _context.Authors
                    .Where(a => a.Name.ToLower() == authorName.ToLower())
                    .Select(a => a.Id)
                    .Contains(ba.Author_Id)
                )
                .Select(ba => _context.Books
                    .FirstOrDefault(b => b.Id == ba.Book_Id)
                ).ToListAsync();

            return books.Count > 0 ? books : null;
        }

        public async Task<IEnumerable<Book?>?> GetBooksByCategory(string categoryName)
        {
            var books = await _context.BookCategories
                .Where(bc => _context.Authors
                    .Where(c => c.Name.ToLower() == categoryName.ToLower())
                    .Select(c => c.Id)
                    .Contains(bc.categoryId)
                )
                .Select(bc => _context.Books
                    .FirstOrDefault(b => b.Id == bc.bookId)
                ).ToListAsync();

            return books?.Count > 0 ? books : null;
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

            var cat = new Category { Id = Guid.NewGuid(), Name = category };

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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
                Language = langName
            };

            await _context.Languages.AddAsync(lang);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task LinkBookToAuthor(Guid bookId, Guid authorId)
        {
            var bookAuthor = new BookAuthors
            {
                Id = Guid.NewGuid(),
                Book_Id = bookId,
                Author_Id = authorId
            };

            await _context.BookAuthors.AddAsync(bookAuthor);
            await _context.SaveChangesAsync();
        }

        public async Task LinkBookToCategory(Guid bookId, Guid categoryId)
        {
            var bookCategory = new BookCategories
            {
                Id = Guid.NewGuid(),
                bookId = bookId,
                categoryId = categoryId
            };

            await _context.BookCategories.AddAsync(bookCategory);
            await _context.SaveChangesAsync();
        }

        public async Task LinkBookToLanguage(Guid bookId, Guid langId)
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
