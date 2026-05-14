using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IBookRepository : IGenericRepo<Book>
    {
        public Task<object?> GetBookWithDetailsAsync(string id);
        public Task<object?> GetBookByName(string bookName);

        public Task<IEnumerable<object?>?> GetBooksByCategory(string categoryName);
        public Task<IEnumerable<object?>?> GetBooksByAuthor(string authorName);
        public Task<IEnumerable<Category?>?> GetCategories();
        public Task<IEnumerable<Author?>?> GetAuthors();
        public Task<IEnumerable<Languages?>?> GetLanguages();
        public Task<bool> AddRating(string BookId, RatingDTO dto);
        public Task<List<BookRating>?> GetBookRatings(string BookId);
        public Task<bool> DeleteRating(string rateId);

        // Add new category/author (does not save changes; caller should call UnitOfWork.CompleteAsync)
        public Task<bool> AddCategory(string category);
        public Task<bool> AddAuthor(string author, string desc = "");
        public Task<bool> AddLanguage(string langName);
        // Link book to category/author/language (does not save changes; caller should call UnitOfWork.CompleteAsync)
        public Task LinkBookToAuthor(string bookId, string authorId);
        public Task LinkBookToCategory(string bookId, string categoryId);
        public Task LinkBookToLanguage(string bookId, string langId);
    }
}
