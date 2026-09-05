using SuttorLib.Models.Blog;

namespace SuttorLib.DTOs
{
    public class CreateBlogDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string>? Tags { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<IFormFile>? Photos { get; set; }
    }

    public class UpdateBlogDTO
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string>? Tags { get; set; } = new List<string>();
        public string? Category { get; set; } = string.Empty;
    }
    
    //For the public responses that will get back from the Controller endpoints
    //with mapping the info that will come back from MySQL DB.
    public class BlogDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string publisherId { get; set; } = string.Empty;
        public string? publisherName { get; set; } = string.Empty;
        public string? publisherUserName { get; set; } = string.Empty;
        public string publisherPic { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public int Views { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public int? CommentCount { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();
    }

    public class CreateCommentDTO
    {
        public string Content { get; set; } = string.Empty;
        public List<string>? Tags { get; set; } = new();
        public string? Category { get; set; }
    }

    public class CommentDTO
    {
        public string Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string CommenterId { get; set; } = string.Empty;
        public string? CommenterFullName { get; set; } = string.Empty;
        public string? CommenterUserName { get; set; } = string.Empty;
        public IFormFile? CommenterPhoto { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public List<string> UserIdsLikes { get; set; } = new();
        public List<string> UserIdsDislikes { get; set; } = new();
        public List<Comment> Replies { get; set; } = new();
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
        public List<BlogCommentResponseDto> Replies { get; set; } = new();
    }

    public class UpdateCommentDto
    {
        public string? Content { get; set; }
        public List<string>? Tags { get; set; } = new();
    }

    public class UpdateReplyDTO
    {
        public string? Content { get; set; }
        public List<string>? Tags { get; set; }
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
        public List<Blog> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedBlogListResponseDto
    {
        public List<BlogDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedCommentResponseDto
    {
        public List<CommentDTO> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
