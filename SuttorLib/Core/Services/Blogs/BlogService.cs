using Microsoft.Extensions.Options;
using MongoDB.Bson;
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

        // ========================  Blog Operations  =====================================
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

        public async Task<PaginatedBlogResponseDto> GetAllBlogs(BlogFilterDto dto)
        {
            var filter = Builders<Models.Blog>.Filter.Eq(b => b.IsPublished, true);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
            {
                var searchFilter = Builders<Models.Blog>.Filter.Or(
                    Builders<Models.Blog>.Filter.Regex(b => b.Title, dto.SearchTerm),
                    Builders<Models.Blog>.Filter.Regex(b => b.Content, dto.SearchTerm)
                );
                filter &= searchFilter;
            }

            // Apply tag filter
            if (!string.IsNullOrWhiteSpace(dto.Tag))
            {
                var tag = NormalizeTags(new List<string> { dto.Tag }).First();
                filter &= Builders<Models.Blog>.Filter.AnyEq(b => b.Tags, tag);
            }

            // Apply publisher filter
            if (!string.IsNullOrWhiteSpace(dto.PublisherId))
            {
                filter &= Builders<Models.Blog>.Filter.Eq(b => b.PublisherId, dto.PublisherId);
            }

            var totalCount = await _blog.CountDocumentsAsync(filter);

            // Apply sorting
            var sort = dto.SortBy.ToLower() switch
            {
                "popular" => Builders<Models.Blog>.Sort.Descending(b => b.Likes),
                "trending" => Builders<Models.Blog>.Sort.Descending(b => b.Views),
                _ => Builders<Models.Blog>.Sort.Descending(b => b.CreatedAt)
            };

            var skip = (dto.Page - 1) * dto.PageSize;
            var blogs = await _blog
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(dto.PageSize)
                .ToListAsync();

            var items = blogs.ToList();

            return new PaginatedBlogResponseDto
            {
                Items = items,
                TotalCount = (int)totalCount,
                Page = dto.Page,
                PageSize = dto.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)dto.PageSize)
            };
        }

        public async Task<GetBlogDTO> UpdateBlogAsync(string blogId, UpdateBlogDTO updateDto, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            if (blog.PublisherId != userId)
                throw new UnauthorizedAccessException("You can only update your own blogs");

            var update = Builders<Models.Blog>.Update
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                update = update.Set(b => b.Title, updateDto.Title);

            if (!string.IsNullOrWhiteSpace(updateDto.Content))
                update = update.Set(b => b.Content, updateDto.Content);

            if (updateDto.Tags != null)
                update = update.Set(b => b.Tags, NormalizeTags(updateDto.Tags));

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            return MapToResponseDto(updatedBlog);
        }

        public async Task<bool> DeleteBlogAsync(string blogId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            if (blog.PublisherId != userId)
                throw new UnauthorizedAccessException("You can only delete your own blogs");

            var result = await _blog.DeleteOneAsync(b => b.Id == objectId);
            return result.DeletedCount > 0;
        
        }

        public async Task<GetBlogDTO> GetBlogById(string blogId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            // Increment views
            var update = Builders<Models.Blog>.Update.Inc(b => b.Views, 1);
            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            return MapToResponseDto(blog);
        }

        public async Task<List<GetBlogDTO>?> GetBlogsByCategory(string category, int page = 1, int pageSize = 10)
        {
            var filter = Builders<Models.Blog>.Filter.Eq(b => b.Category, category);
            var totalCount = await _blog.CountDocumentsAsync(filter);

            var skip = (page - 1) * pageSize;
            var blogs = await _blog
                .Find(filter)
                .Sort(Builders<Models.Blog>.Sort.Descending(b => b.CreatedAt))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            var items = blogs.ToList();

            return new PaginatedBlogResponseDto
            {
                Items = items,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<List<GetBlogDTO>?> GetBlogsByPublisher(string userId, int page = 1, int pageSize = 10)
        {
            var filter = Builders<Models.Blog>.Filter.Eq(b => b.PublisherId, userId);
            var totalCount = await _blog.CountDocumentsAsync(filter);

            var skip = (page - 1) * pageSize;
            var blogs = await _blog
                .Find(filter)
                .Sort(Builders<Models.Blog>.Sort.Descending(b => b.CreatedAt))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            var items = blogs.ToList();

            return new PaginatedBlogResponseDto
            {
                Items = items,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
        
        public async Task<List<GetBlogDTO>?> SearchBlogsByTags(List<string> tags, int page = 1, int pageSize = 10)
        {
            var normalizedTag = NormalizeTags(tags).First();
            var filter = Builders<Models.Blog>.Filter.AnyEq(b => b.Tags, normalizedTag);

            var totalCount = await _blog.CountDocumentsAsync(filter);

            var skip = (page - 1) * pageSize;
            var blogs = await _blog
                .Find(filter)
                .Sort(Builders<Models.Blog>.Sort.Descending(b => b.CreatedAt))
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            var items = blogs.ToList();

            return new PaginatedBlogResponseDto
            {
                Items = items,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        // ========================  Comment Operations  =====================================

        public async Task<BlogCommentDTO> CreateCommentAsync(string blogId, string userId, CreateCommentDTO commentDTO)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var comment = new Comment
            {
                Content = commentDTO.Content,
                CommenterId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            var update = Builders<Models.Blog>.Update
                .Push(b => b.Comments, comment)
                .Inc(b => b.Views, 1);

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            return MapToCommentResponseDto(comment);
        }
        
        public async Task<BlogCommentDTO> GetCommentById(string commentId)
        {
            if (!ObjectId.TryParse(commentId, out var comId))
                throw new ArgumentException("Invalid comment ID");

            var comment = await _comment.Find(c => c.Id == comId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            return MapToCommentResponseDto(comment);
        }
        
        public async Task<List<BlogCommentDTO>> GetCommentsAsync(string blogId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            return blog.Comments.Select(MapToCommentResponseDto).ToList();
        }
        
        public async Task<BlogCommentDTO> UpdateComment(string commentId, UpdateCommentDto updateDTO, string userId)
        {
            if (!ObjectId.TryParse(updateDTO.blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            if (string.IsNullOrWhiteSpace(updateDTO.Content))
                throw new ArgumentException("Content cannot be empty");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var comment = blog.Comments.FirstOrDefault(c => c.Id == commentObjectId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.CommenterId != userId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            comment.Content = updateDTO.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            var update = Builders<Models.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return MapToCommentResponseDto(comment);
        }

        public async Task<bool> DeleteCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var comment = blog.Comments.FirstOrDefault(c => c.Id == commentObjectId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.CommenterId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            var update = Builders<Models.Blog>.Update.PullFilter(b => b.Comments, c => c.Id == commentObjectId);
            var result = await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return result.ModifiedCount > 0;
        }


        // ========================  Like/Dislike Operations  =====================================

        public async Task<LikeDislikeResponseDto> LikeBlogAsync(string blogId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var updateBuilder = Builders<Models.Blog>.Update;
            UpdateDefinition<Models.Blog> update;

            if (blog.UserLikes.Contains(userId))
            {
                update = updateBuilder.Combine(
                    updateBuilder.Pull(b => b.UserLikes, userId),
                    updateBuilder.Inc(b => b.Likes, -1)
                );
            }
            else
            {
                var updates = new List<UpdateDefinition<Models.Blog>>();

                if (blog.UserDislikes.Contains(userId))
                {
                    updates.Add(updateBuilder.Pull(b => b.UserDislikes, userId));
                    updates.Add(updateBuilder.Inc(b => b.Dislikes, -1));
                }

                updates.Add(updateBuilder.Push(b => b.UserLikes, userId));
                updates.Add(updateBuilder.Inc(b => b.Likes, 1));

                update = updateBuilder.Combine(updates);
            }

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();

            return MapToLikeDislikeResponseDto(updatedBlog, userId);
        }

        public async Task<LikeDislikeResponseDto> RemoveLikeFromBlogAsync(string blogId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var update = Builders<Models.Blog>.Update
                .Pull(b => b.UserLikes, userId)
                .Inc(b => b.Likes, -1);

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            return MapToLikeDislikeResponseDto(updatedBlog, userId);
        }
        
        public async Task<LikeDislikeResponseDto> DislikeBlogAsync(string blogId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var updateBuilder = Builders<Models.Blog>.Update;
            UpdateDefinition<Models.Blog> update;

            if (blog.UserLikes.Contains(userId))
            {
                update = updateBuilder.Combine(
                    updateBuilder.Pull(b => b.UserDislikes, userId),
                    updateBuilder.Inc(b => b.Dislikes, -1)
                );
            }
            else
            {
                var updates = new List<UpdateDefinition<Models.Blog>>();

                if (blog.UserLikes.Contains(userId))
                {
                    updates.Add(updateBuilder.Pull(b => b.UserLikes, userId));
                    updates.Add(updateBuilder.Inc(b => b.Likes, -1));
                }

                updates.Add(updateBuilder.Push(b => b.UserDislikes, userId));
                updates.Add(updateBuilder.Inc(b => b.Dislikes, 1));

                update = updateBuilder.Combine(updates);
            }

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();

            return MapToLikeDislikeResponseDto(updatedBlog, userId);
        }
        
        public async Task<LikeDislikeResponseDto> RemoveDislikeFromBlogAsync(string blogId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var update = Builders<Models.Blog>.Update
                .Pull(b => b.UserDislikes, userId)
                .Inc(b => b.Dislikes, -1);

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            return MapToLikeDislikeResponseDto(updatedBlog, userId);
        }

        public async Task<LikeDislikeResponseDto> LikeCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var comment = blog.Comments.FirstOrDefault(c => c.Id == commentObjectId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserLikes.Contains(userId))
            {
                comment.UserLikes.Remove(userId);
                comment.Likes--;
            }
            else
            {
                if (comment.UserDislikes.Contains(userId))
                {
                    comment.UserDislikes.Remove(userId);
                    comment.Dislikes--;
                }

                comment.UserLikes.Add(userId);
                comment.Likes++;
            }

            var update = Builders<Models.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserLikes.Contains(userId),
                UserDisliked = comment.UserDislikes.Contains(userId)
            };
        }
        
        public async Task<LikeDislikeResponseDto> DislikeCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var comment = blog.Comments.FirstOrDefault(c => c.Id == commentObjectId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserDislikes.Contains(userId))
            {
                comment.UserDislikes.Remove(userId);
                comment.Dislikes--;
            }
            else
            {
                if (comment.UserLikes.Contains(userId))
                {
                    comment.UserLikes.Remove(userId);
                    comment.Likes--;
                }

                comment.UserDislikes.Add(userId);
                comment.Dislikes++;
            }

            var update = Builders<Models.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserLikes.Contains(userId),
                UserDisliked = comment.UserDislikes.Contains(userId)
            };
        }
        
        public async Task<LikeDislikeResponseDto> RemoveLikeFromCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var comment = blog.Comments.FirstOrDefault(c => c.Id == commentObjectId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserLikes.Contains(userId))
            {
                comment.UserLikes.Remove(userId);
                comment.Likes--;
            }

            var update = Builders<Models.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserLikes.Contains(userId),
                UserDisliked = comment.UserDislikes.Contains(userId)
            };
        }
        
        public async Task<LikeDislikeResponseDto> RemoveDislikeFromCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            var comment = blog.Comments.FirstOrDefault(c => c.Id == commentObjectId);
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.UserDislikes.Contains(userId))
            {
                comment.UserDislikes.Remove(userId);
                comment.Dislikes--;
            }

            var update = Builders<Models.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserLikes.Contains(userId),
                UserDisliked = comment.UserDislikes.Contains(userId)
            };
        }
        


        //========================  Helper Methods  =====================================

        private GetBlogDTO MapToResponseDto(Models.Blog blog)
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
                Comments = blog.Comments
            };
        }

        private BlogCommentDTO MapToCommentResponseDto(Comment comment)
        {
            return new BlogCommentDTO
            {
                Id = comment.Id.ToString(),
                Content = comment.Content,
                CommenterId = comment.CommenterId,
                CreatedAt = comment.CreatedAt,
                Tags = comment.Tags,
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLikes = comment.UserLikes,
                UserDislikes = comment.UserDislikes,
                Replies = comment.Replies.Select(MapToCommentResponseDto).ToList()
            };
        }

        private List<string> NormalizeTags(List<string> tags)
        {
            return tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().StartsWith("#") ? t.Trim() : $"#{t.Trim()}")
                .Distinct()
                .ToList();
        }

        private LikeDislikeResponseDto MapToLikeDislikeResponseDto(Models.Blog blog, string userId)
        {
            return new LikeDislikeResponseDto
            {
                LikesCount = blog.Likes,
                DislikesCount = blog.Dislikes,
                UserLiked = blog.UserLikes.Contains(userId),
                UserDisliked = blog.UserDislikes.Contains(userId)
            };
        }
    }
}
