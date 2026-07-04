using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IBookRepository : IGenericRepo<Book>
    {
        public Task<PagedResult<BookListItemDto>> GetBooksPagedAsync(int page, int pageSize, string? search);
        public Task<object?> GetBookWithDetailsAsync(string id);
        public Task<object?> GetBookByName(string bookName);
        public Task<IEnumerable<object?>?> GetBooksByCategory(string categoryName);
        public Task<IEnumerable<object?>?> GetBooksByAuthor(string authorName);
        public Task<IEnumerable<Category?>?> GetCategories();
        public Task<IEnumerable<Author?>?> GetAuthors();
        public Task<IEnumerable<Languages?>?> GetLanguages();

        public Task<bool> AddRating(string BookId, RatingDTO dto, string userId);
        public Task<List<BookRating>?> GetBookRatings(string BookId);
        public Task<bool> DeleteRating(string rateId, string userId);

        // Add new category/author (does not save changes; caller should call UnitOfWork.CompleteAsync)
        public Task<Category?> AddCategory(string category);
        public Task<Author?> AddAuthor(string Id, string author, string desc = "", bool IsReg = false, string? Photo =  "");
        public Task<Languages?> AddLanguage(string langName);

        // Link book to category/author/language (does not save changes; caller should call UnitOfWork.CompleteAsync)
        public Task LinkBookToAuthor(string bookId, string authorId);
        public Task LinkBookToCategory(string bookId, string categoryId);
        public Task<bool> IsFinishReading(string bookId);

        // Update Authot and Category
        public Task<Category?> updateCategory(string catId, string catName, string catIcon);
        public Task<Author?> updateAuthor(string authId, string authName, string authPhoto, string authDesc);
    }
}
