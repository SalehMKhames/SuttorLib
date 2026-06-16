using SuttorLib.DTOs;

namespace SuttorLib.Core.Services.Blog
{
    public interface IBlogServices
    {
        //Basic Blog operations
        public Task<GetBlogDTO> CreateBlogAsync(string userId, CreateBlogDTO blogDTO);
        public Task<PaginatedBlogResponseDto> GetAllBlogs(BlogFilterDto filterDto);
        public Task<GetBlogDTO> GetBlogById(string blogId);
        public Task<GetBlogDTO> UpdateBlogAsync(string blogId, UpdateBlogDTO blogDTO, string userId);
        public Task<bool> DeleteBlogAsync(string blogId, string userId);

        //Some specified Gets
        public Task<List<GetBlogDTO>?> GetBlogsByPublisher(string userId, int page = 1, int pageSize = 10);
        public Task<List<GetBlogDTO>?> SearchBlogsByTags(List<string> tags, int page =1, int pageSize = 10);
        public Task<List<GetBlogDTO>?> GetBlogsByCategory(string category, int page = 1, int pageSize = 10);

        //Basic Comment operations
        public Task<BlogCommentDTO> CreateCommentAsync(string blogId, string userId, CreateCommentDTO commentDTO);
        Task<List<BlogCommentDTO>> GetCommentsAsync(string blogId);
        public Task<BlogCommentDTO> GetCommentById(string commentId);
        public Task<BlogCommentDTO> UpdateComment(string commentId, UpdateCommentDto updateDTO, string userId);
        public Task<bool> DeleteCommentAsync(string blogId, string commentId, string userId);

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
