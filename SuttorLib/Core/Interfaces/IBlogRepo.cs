using SuttorLib.DTOs;

namespace SuttorLib.Core.Interfaces
{
    public interface IBlogRepo
    {
        //Blog Operations
        Task<string> CreateBlogAsync(string userId, CreateBlogDTO blog);
        Task<GetBlogDTO?> GetBlogAsync(string id);

    }
}
