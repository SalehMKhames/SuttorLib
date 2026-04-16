using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IUserRepository : IGenericRepo<AppUser>
    {
        public Task<AppUser?> GetUserByEmail(string email);
        public Task<AppUser?> GetUserByUsername(string username);
        public Task<bool?> AddUserInterest(UserInterestDTO dto);
        public Task<List<Category?>?> UpdateUserInterest(UserInterestDTO dto);
    }
}
