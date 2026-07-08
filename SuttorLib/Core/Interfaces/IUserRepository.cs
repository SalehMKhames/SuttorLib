using SuttorLib.Models.Library;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IUserRepository : IGenericRepo<AppUser>
    {
        public Task<GetPublicUserDTO?> GetUserByEmail(string email);
        public Task<GetPublicUserDTO?> GetUserByUsername(string username);
        public Task<bool?> AddUserInterest(UserInterestDTO dto);
        public Task<List<Category?>?> UpdateUserInterest(UserInterestDTO dto);
        public Task<bool> PromoteToAuthor(AppUser user, int xp);
    }
}
