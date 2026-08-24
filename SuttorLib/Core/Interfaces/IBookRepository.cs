using SuttorLib.Models.Library;
using SuttorLibrary.DTOs;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IBookRepository : IGenericRepo<Book>
    {
        public Task<PagedResult<BookListItemDto>> GetBooksPagedAsync(int page, int pageSize, string? search);
        public Task<object?> GetBookWithDetailsAsync(string id, string? userId);
        public Task<object?> GetBookByName(string bookName);

        public Task<PagedResult<BookListItemDto>> GetBooksByCategoryPagedAsync(string categoryName, int page, int pageSize);
        public Task<PagedResult<BookListItemDto>> GetBooksByAuthorPagedAsync(string authorName, int page, int pageSize);


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
        public Task<bool> IsFinishReading(string bookId, string userId);

        // Update Authot and Category
        public Task<Category?> updateCategory(string catId, string catName, string catIcon);
        public Task<Author?> updateAuthor(string authId, string authName, string authPhoto, string authDesc);

        //Favorite Books
        public Task<bool> AddFavoriteBook(string bookId, string userId);
        public Task<bool> RemoveFavoriteBook(string bookId, string userId);
        public Task<IEnumerable<object>?> GetFavoriteBooks(string userId);
    }
}
