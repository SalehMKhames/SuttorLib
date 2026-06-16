using SuttorLib.DTOs;

namespace SuttorLib.Core.Services.Blog
{
    public interface IBlogServices
    {
        //Basic Blog operations
        public Task<GetBlogDTO> CreateBlogAsync(string userId, CreateBlogDTO blogDTO);
        public Task<GetBlogDTO> GetAllBlogs();
        public Task<GetBlogDTO> GetBlogById(string blogId);
        public Task<GetBlogDTO> UpdateBlogAsync(string blogId, UpdateBlogDTO blogDTO);
        public Task<bool> DeleteBlogAsync(string blogId);

        //Some specified Gets
        public Task<List<GetBlogDTO>?> GetBlogsByPublisher(string userId);
        public Task<List<GetBlogDTO>?> SearchBlogsByTags(List<string> tags, int page =1, int pageSize = 10);
        public Task<List<GetBlogDTO>?> GetBlogsByCategory(string category, int page = 1, int pageSize = 10);

        //Basic Comment operations
        public Task CreateCommentAsync(string userId, CreateCommentDTO commentDTO);
        Task<List<BlogCommentResponseDto>> GetCommentsAsync(string blogId);
        public Task<BlogCommentResponseDto> GetCommentById(string commentId);
        public Task<BlogCommentResponseDto> UpdateComment(string commentId, UpdateCommentDto commentDTO);
        public Task<bool> DeleteCommentAsync(string commentId);

        // Like/Dislike Operations
        Task<LikeDislikeResponseDto> LikeBlogAsync(string blogId, string userId);
        Task<LikeDislikeResponseDto> DislikeBlogAsync(string blogId, string userId);
        Task<LikeDislikeResponseDto> RemoveLikeFromBlogAsync(string blogId, string userId);
        Task<LikeDislikeResponseDto> RemoveDislikeFromBlogAsync(string blogId, string userId);

        // Comment Like/Dislike
        Task<LikeDislikeResponseDto> LikeCommentAsync(string blogId, string commentId, string userId);
        Task<LikeDislikeResponseDto> DislikeCommentAsync(string blogId, string commentId, string userId);
        Task<LikeDislikeResponseDto> RemoveLikeFromCommentAsync(string blogId, string commentId, string userId);
        Task<LikeDislikeResponseDto> RemoveDislikeFromCommentAsync(string blogId, string commentId, string userId);
    }
}
