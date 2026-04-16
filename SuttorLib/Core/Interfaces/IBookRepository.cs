using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IBookRepository : IGenericRepo<Book>
    {
        public Task<IQueryable> GetByIdFromQuery(string id);
        public Task<Book?> GetBookByName(string bookName);

        public Task<IEnumerable<Book?>?> GetBooksByCategory(string categoryName);
        public Task<IEnumerable<Book?>?> GetBooksByAuthor(string authorName);
        public Task<IEnumerable<Category?>?> GetCategories();
        public Task<IEnumerable<Author?>?> GetAuthors();
        public Task<IEnumerable<Languages?>?> GetLanguages();

        // Add new category/author (does not save changes; caller should call UnitOfWork.CompleteAsync)
        public Task<bool> AddCategory(string category);
        public Task<bool> AddAuthor(string author, string desc = "");
        public Task<bool> AddLanguage(string langName);
        // Link book to category/author/language (does not save changes; caller should call UnitOfWork.CompleteAsync)
        public Task LinkBookToAuthor(Guid bookId, Guid authorId);
        public Task LinkBookToCategory(Guid bookId, Guid categoryId);
        public Task LinkBookToLanguage(Guid bookId, Guid langId);
    }
}
