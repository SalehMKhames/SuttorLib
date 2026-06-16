using MongoDB.Bson.Serialization.Attributes;
using SuttorLib.Models;

namespace SuttorLib.DTOs
{
    public class CreateBlogDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public string Category { get; set; } = string.Empty;
    }

    public class UpdateBlogDTO
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? Tags { get; set; } = new List<string>();
        public string? Category { get; set; } = string.Empty;
    }

    public class GetBlogDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string publisherId { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Views { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public string Category { get; set; } = string.Empty;
    }

    public class CreateCommentDTO
    {
        public string Content { get; set; } = string.Empty;
        public List<string>? Tags { get; set; } = new();
    }

    public class BlogCommentResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CommenterId { get; set; } = string.Empty;
        public string CommenterName { get; set; } = string.Empty;
        public string? CommenterPhoto { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public bool CurrentUserLiked { get; set; }
        public bool CurrentUserDisliked { get; set; }
        public List<BlogCommentResponseDto> Replies { get; set; } = new();
    }
    // For nested comments (Replies), we can use the same DTO
    // But with an additional property for the parent comment ID
    public class BlogCommentDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string CommenterId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public bool CurrentUserLiked { get; set; }
        public bool CurrentUserDisliked { get; set; }
        public List<BlogCommentDTO> Replies { get; set; } = new();
    }

    public class UpdateCommentDto
    {
        public string? Content { get; set; }
        public List<string>? Tags { get; set; } = new();
    }

    public class LikeDislikeResponseDto
    {
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public bool UserLiked { get; set; }
        public bool UserDisliked { get; set; }
    }

    // Pagination Request
    public class BlogFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Tag { get; set; }
        public string? PublisherId { get; set; }
        public string SortBy { get; set; } = "recent"; // recent, popular, trending
    }

    public class PaginatedBlogResponseDto
    {
        public List<BlogListItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
