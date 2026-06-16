using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SuttorLib.Core.Services.Blog;
using SuttorLib.Data;
using SuttorLib.DTOs;
using SuttorLib.Models;

namespace SuttorLib.Core.Services.Blogs
{
    public class BlogService : IBlogServices
    {
        private readonly IMongoCollection<Models.Blog> _blog;
        private readonly IMongoCollection<Comment> _comment;
        private readonly IOptions<BlogDbSettings> _dbSettings;
        private readonly IHttpContextAccessor _httpContext;

        public BlogService(IOptions<BlogDbSettings> dbSettings, IHttpContextAccessor httpContext)
        {
            _dbSettings = dbSettings;
            _httpContext = httpContext;

            var mongoClient = new MongoClient(_dbSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(_dbSettings.Value.DatabaseName);

            _blog = mongoDatabase.GetCollection<Models.Blog>(_dbSettings.Value.BlogCollection);
            _comment = mongoDatabase.GetCollection<Comment>(_dbSettings.Value.CommentCollection);
        }

        public async Task<GetBlogDTO> CreateBlogAsync(string userId, CreateBlogDTO createDTO)
        {
            var blog = new Models.Blog 
            {
                Title = createDTO.Title,
                Content = createDTO.Content,
                CreatedAt = DateTime.UtcNow,
                PublisherId = userId,
                Likes = 0,
                Dislikes = 0,
                Tags = createDTO.Tags,
                Views = 0,
                Category = createDTO.Category
            };

            await _blog.InsertOneAsync(blog);
            
            return MapToResponseDto(blog);
        }

        public Task CreateCommentAsync(string userId, CreateCommentDTO commentDTO)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteBlogAsync(string blogId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCommentAsync(string commentId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> DislikeBlogAsync(string blogId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> DislikeCommentAsync(string blogId, string commentId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<GetBlogDTO> GetAllBlogs()
        {
            throw new NotImplementedException();
        }

        public Task<GetBlogDTO> GetBlogById(string blogId)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetBlogDTO>?> GetBlogsByCategory(string category, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetBlogDTO>?> GetBlogsByPublisher(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<BlogCommentResponseDto> GetCommentById(string commentId)
        {
            throw new NotImplementedException();
        }

        public Task<List<BlogCommentResponseDto>> GetCommentsAsync(string blogId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> LikeBlogAsync(string blogId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> LikeCommentAsync(string blogId, string commentId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> RemoveDislikeFromBlogAsync(string blogId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> RemoveDislikeFromCommentAsync(string blogId, string commentId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> RemoveLikeFromBlogAsync(string blogId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<LikeDislikeResponseDto> RemoveLikeFromCommentAsync(string blogId, string commentId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<GetBlogDTO>?> SearchBlogsByTags(List<string> tags, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<GetBlogDTO> UpdateBlogAsync(string blogId, UpdateBlogDTO blogDTO)
        {
            throw new NotImplementedException();
        }

        public Task<BlogCommentResponseDto> UpdateComment(string commentId, UpdateCommentDto commentDTO)
        {
            throw new NotImplementedException();
        }


        //========================  Helper Methods  =====================================

        private GetBlogDTO MapToResponseDto(Models.Blog blog, string? currentUserId = null)
        {
            return new GetBlogDTO
            {
                Id = blog.Id.ToString(),
                Title = blog.Title,
                Content = blog.Content,
                Tags = blog.Tags,
                CreatedAt = blog.CreatedAt,
                Likes = blog.Likes,
                Dislikes = blog.Dislikes,
                Views = blog.Views,
                Comments = blog.Comments.Select(c => MapToCommentResponseDto(c, currentUserId)).ToList()
            };
        }

        private BlogCommentResponseDto MapToCommentResponseDto(Comment comment, string? currentUserId = null)
        {
            return new BlogCommentResponseDto
            {
                Id = comment.Id.ToString(),
                Content = comment.Content,
                CommenterId = comment.CommenterId,

                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.CreatedAt,
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                
                Replies = comment.Replies.Select(r => MapToCommentResponseDto(r, currentUserId)).ToList()
            };
        }
    }
}
