using SuttorLib.DTOs;
using SuttorLib.Models.Blog;

namespace SuttorLib.Core.Services.Blog
{
    public interface IBlogServices
    {
        //Basic Blog operations
        public Task<Models.Blog.Blog> CreateBlogAsync(string userId, CreateBlogDTO blogDTO, List<string> photos);
        public Task<PaginatedBlogResponseDto> GetAllBlogs(BlogFilterDto filterDto);
        public Task<Models.Blog.Blog> GetBlogById(string blogId);
        public Task<Models.Blog.Blog> UpdateBlogAsync(string blogId, UpdateBlogDTO blogDTO, string userId);
        public Task<bool> DeleteBlogAsync(string blogId, string userId);

        //Some specified Gets
        public Task<PaginatedBlogResponseDto?> GetBlogsByPublisher(string userId, int page = 1, int pageSize = 10);
        public Task<PaginatedBlogResponseDto?> SearchBlogsByTags(List<string> tags, int page =1, int pageSize = 10);
        public Task<PaginatedBlogResponseDto?> GetBlogsByCategory(string category, int page = 1, int pageSize = 10);

        //Basic Comment operations
        public Task<Comment> CreateCommentAsync(string blogId, string userId, CreateCommentDTO commentDTO);
        Task<List<Comment>?> GetCommentsAsync(string blogId);
        public Task<Comment> GetCommentById(string commentId);
        public Task<Comment> UpdateComment(string commentId, UpdateCommentDto updateDTO, string userId);
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
