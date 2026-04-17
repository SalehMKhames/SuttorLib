using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Interfaces
{
    public interface IUserRepository : IGenericRepo<AppUser>
    {
        public Task<GetUserDTO?> GetUserByEmail(string email);
        public Task<GetUserDTO?> GetUserByUsername(string username);
        public Task<bool?> AddUserInterest(UserInterestDTO dto);
        public Task<List<Category?>?> UpdateUserInterest(UserInterestDTO dto);
    }
}
