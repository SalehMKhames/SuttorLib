using SuttorLib.Models.Library;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IUserRepository : IGenericRepo<AppUser>
    {
        public Task<GetPublicUserDTO?> GetUserByEmail(string email);
        public Task<GetPublicUserDTO?> GetUserByUsername(string username);
        public Task<bool> AddUserInterest(string userId, List<string> categoryNames);
        public Task<List<Category?>?> GetUserInterests(string userId);
        public Task<List<Category?>?> UpdateUserInterest(string userId, List<string> categoryNames);
        public Task<bool> PromoteToAuthor(AppUser user, int xp);
        public Task<List<BookListItemDto>?> SuggestedBooks(string userId);
        public Task<List<AppUser>?> GetUsersByInterests(string categoryId);
    }
}
