using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using SuttorLib.Core.Services.Blog;
using SuttorLib.Data;
using SuttorLib.DTOs;
using SuttorLib.Models.Blog;
using System.Text.RegularExpressions;

namespace SuttorLib.Core.Services.Blogs
{
    public class BlogService : IBlogServices
    {
        private readonly IMongoCollection<Models.Blog.Blog> _blog;
        private readonly IMongoCollection<Comment> _comment;
        private readonly IOptions<BlogDbSettings> _dbSettings;
        private readonly IHttpContextAccessor _httpContext;

        public BlogService(IOptions<BlogDbSettings> dbSettings, IHttpContextAccessor httpContext)
        {
            _dbSettings = dbSettings;
            _httpContext = httpContext;

            var cs = _dbSettings?.Value?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("BlogDbSettings.ConnectionString is missing. Ensure configuration binds the 'BlogDbSettings' section.");

            var mongoClient = new MongoClient(cs);
            var mongoDatabase = mongoClient.GetDatabase(_dbSettings?.Value?.DatabaseName);

            _blog = mongoDatabase.GetCollection<Models.Blog.Blog>(_dbSettings?.Value?.BlogCollection);
            _comment = mongoDatabase.GetCollection<Comment>(_dbSettings?.Value?.CommentCollection);
        }

        // ========================  Blog Operations  =====================================
        public async Task<Models.Blog.Blog> CreateBlogAsync(string userId, CreateBlogDTO createDTO, List<string> photosPath)
        {
            var blog = new Models.Blog.Blog
            {
                Title = createDTO.Title,
                Content = createDTO.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublisherId = userId,
                Likes = 0,
                Dislikes = 0,
                Tags = NormalizeTags(createDTO.Tags ?? new List<string>()),
                Views = 0,
                Category = createDTO.Category,
                IsPublished = true,
                UserIdsLikes = new List<string>(),
                UserIdsDislikes = new List<string>(),
                Comments = new List<ObjectId>(),
                Photos = photosPath
            };

            await _blog.InsertOneAsync(blog);
            
            return blog;
        }

        public async Task<PaginatedBlogResponseDto> GetAllBlogs(BlogFilterDto dto)
        {
            // Clamp pagination input
            var page = Math.Max(1, dto.Page);
            var pageSize = Math.Clamp(dto.PageSize, 1, 50);

            var filter = Builders<Models.Blog.Blog>.Filter.Eq(b => b.IsPublished, true);

            // Apply search filter (escaped so user input can't break or abuse the regex)
            if (!string.IsNullOrWhiteSpace(dto.SearchTerm))
            {
                var term = Regex.Escape(dto.SearchTerm.Trim());
                var searchFilter = Builders<Models.Blog.Blog>.Filter.Or(
                    Builders<Models.Blog.Blog>.Filter.Regex(b => b.Title, new BsonRegularExpression(term, "i")),
                    Builders<Models.Blog.Blog>.Filter.Regex(b => b.Content, new BsonRegularExpression(term, "i"))
                );
                filter &= searchFilter;
            }

            // Apply tag filter
            if (!string.IsNullOrWhiteSpace(dto.Tag))
            {
                var tag = NormalizeTags(new List<string> { dto.Tag }).First();
                filter &= Builders<Models.Blog.Blog>.Filter.AnyEq(b => b.Tags, tag);
            }

            // Apply publisher filter
            if (!string.IsNullOrWhiteSpace(dto.PublisherId))
            {
                filter &= Builders<Models.Blog.Blog>.Filter.Eq(b => b.PublisherId, dto.PublisherId);
            }

            var totalCount = await _blog.CountDocumentsAsync(filter);

            var sort = dto.SortBy.ToLower() switch
            {
                "popular" => Builders<Models.Blog.Blog>.Sort.Descending(b => b.Likes),
                "trending" => Builders<Models.Blog.Blog>.Sort.Descending(b => b.Views),
                _ => Builders<Models.Blog.Blog>.Sort.Descending(b => b.CreatedAt)
            };

            var blogs = await _blog
                .Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PaginatedBlogResponseDto
            {
                Items = blogs,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<Models.Blog.Blog> UpdateBlogAsync(string blogId, UpdateBlogDTO updateDto, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog is null)
                throw new KeyNotFoundException("Blog not found");

            if (blog.PublisherId != userId)
                throw new UnauthorizedAccessException("You can only update your own blogs");

            var update = Builders<Models.Blog.Blog>.Update
                .Set(b => b.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                update = update.Set(b => b.Title, updateDto.Title);

            if (!string.IsNullOrWhiteSpace(updateDto.Content))
                update = update.Set(b => b.Content, updateDto.Content);

            if (updateDto.Tags != null)
                update = update.Set(b => b.Tags, NormalizeTags(updateDto.Tags));

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            return updatedBlog;
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
            return result.IsAcknowledged;
        
        }

        public async Task<Models.Blog.Blog> GetBlogById(string blogId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            // Increment views
            var update = Builders<Models.Blog.Blog>.Update.Inc(b => b.Views, 1);
            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            return blog;
        }

        private async Task<PaginatedBlogResponseDto> QueryBlogsAsync(
            FilterDefinition<Models.Blog.Blog> filter, int page, int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            // Only published blogs are returned from list endpoints.
            filter &= Builders<Models.Blog.Blog>.Filter.Eq(b => b.IsPublished, true);

            var totalCount = await _blog.CountDocumentsAsync(filter);

            var blogs = await _blog
                .Find(filter)
                .Sort(Builders<Models.Blog.Blog>.Sort.Descending(b => b.CreatedAt))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PaginatedBlogResponseDto
            {
                Items = blogs,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public Task<PaginatedBlogResponseDto> GetBlogsByCategory(string category, int page = 1, int pageSize = 10)
            => QueryBlogsAsync(Builders<Models.Blog.Blog>.Filter.Eq(b => b.Category, category), page, pageSize);

        public Task<PaginatedBlogResponseDto> GetBlogsByPublisher(string userId, int page = 1, int pageSize = 10)
            => QueryBlogsAsync(Builders<Models.Blog.Blog>.Filter.Eq(b => b.PublisherId, userId), page, pageSize);

        public Task<PaginatedBlogResponseDto> SearchBlogsByTags(List<string> tags, int page = 1, int pageSize = 10)
        {
            if (tags is null || tags.Count == 0)
                throw new ArgumentException("Tags list cannot be empty", nameof(tags));

            // Match blogs containing ANY of the supplied tags (previously only the first tag was used).
            var normalized = NormalizeTags(tags);
            return QueryBlogsAsync(Builders<Models.Blog.Blog>.Filter.AnyIn(b => b.Tags, normalized), page, pageSize);
        }

        // ========================  Comment Operations  =====================================

        public async Task<Comment> CreateCommentAsync(string blogId, string userId, CreateCommentDTO commentDTO)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            if (string.IsNullOrWhiteSpace(commentDTO.Content))
                throw new ArgumentException("Comment content cannot be empty");


            var comment = new Comment
            {
                Id = ObjectId.GenerateNewId(),
                Content = commentDTO.Content,
                CommenterId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tags = commentDTO.Tags ?? new List<string>(),
                Likes = 0,
                Dislikes = 0,
                UserIdsLikes = new List<string>(),
                UserIdsDislikes = new List<string>(),
                Replies = new List<Comment>()
            };

            await _comment.InsertOneAsync(comment);

            var update = Builders<Models.Blog.Blog>.Update
                .Push(b => b.Comments, comment.Id);

            var result = await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            if (result.ModifiedCount == 0)
            {
                await _comment.DeleteOneAsync(c => c.Id == comment.Id);
                throw new KeyNotFoundException("Blog not found");
            }
            return comment;
        }
        
        public async Task<Comment> GetCommentById(string commentId)
        {
            if (!ObjectId.TryParse(commentId, out var comId))
                throw new ArgumentException("Invalid comment ID");

            var comment = await _comment.Find(c => c.Id == comId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            return comment;
        }

        public async Task<List<Comment>?> GetCommentsAsync(string blogId)
        {
            if (!ObjectId.TryParse(blogId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog is null)
                throw new KeyNotFoundException("Blog not found");

            if (blog.Comments is null || blog.Comments.Count == 0)
                return new List<Comment>();

            return await _comment
                .Find(c => blog.Comments.Contains(c.Id))
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> UpdateComment(string commentId, UpdateCommentDto updateDTO, string userId)
        {
            if (!ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid comment ID");

            if (string.IsNullOrWhiteSpace(updateDTO.Content))
                throw new ArgumentException("Content cannot be empty");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            if (comment.CommenterId != userId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            comment.Content = updateDTO.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            if (updateDTO.Tags is not null && updateDTO.Tags.Count == 0)
                comment.Tags = updateDTO.Tags;
            
            var update = Builders<Comment>.Update.Set(c => c, comment);
            await _comment.UpdateOneAsync(b => b.Id == commentObjectId, update);

            return comment;
        }

        public async Task<bool> DeleteCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            ObjectId? com = blog.Comments.Find(c => c == commentObjectId);
            if (com is null)
                throw new KeyNotFoundException("Comment not found");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");
            if (comment.CommenterId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            await _comment.DeleteOneAsync(c => c.Id == commentObjectId);

            blog.Comments.Remove(commentObjectId);

            var update = Builders<Models.Blog.Blog>.Update.PullFilter(b => b.Comments, c => c == commentObjectId);
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

            var updateBuilder = Builders<Models.Blog.Blog>.Update;
            UpdateDefinition<Models.Blog.Blog> update;

            if (blog.UserIdsLikes.Contains(userId))
            {
                update = updateBuilder.Combine(
                    updateBuilder.Pull(b => b.UserIdsLikes, userId),
                    updateBuilder.Inc(b => b.Likes, -1)
                );
            }
            else
            {
                var updates = new List<UpdateDefinition<Models.Blog.Blog>>();

                if (blog.UserIdsDislikes.Contains(userId))
                {
                    updates.Add(updateBuilder.Pull(b => b.UserIdsDislikes, userId));
                    updates.Add(updateBuilder.Inc(b => b.Dislikes, -1));
                }

                updates.Add(updateBuilder.Push(b => b.UserIdsLikes, userId));
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

            var blog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            // ✅ Only remove if user actually liked it
            if (!blog.UserIdsLikes.Contains(userId))
                return MapToLikeDislikeResponseDto(blog, userId); // No change

            var update = Builders<Models.Blog.Blog>.Update
                .Pull(b => b.UserIdsLikes, userId)
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

            var updateBuilder = Builders<Models.Blog.Blog>.Update;
            UpdateDefinition<Models.Blog.Blog> update;

            if (blog.UserIdsLikes.Contains(userId))
            {
                update = updateBuilder.Combine(
                    updateBuilder.Pull(b => b.UserIdsDislikes, userId),
                    updateBuilder.Inc(b => b.Dislikes, -1)
                );
            }
            else
            {
                var updates = new List<UpdateDefinition<Models.Blog.Blog>>();

                if (blog.UserIdsLikes.Contains(userId))
                {
                    updates.Add(updateBuilder.Pull(b => b.UserIdsLikes, userId));
                    updates.Add(updateBuilder.Inc(b => b.Likes, -1));
                }

                updates.Add(updateBuilder.Push(b => b.UserIdsDislikes, userId));
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

            var update = Builders<Models.Blog.Blog>.Update
                .Pull(b => b.UserIdsDislikes, userId)
                .Inc(b => b.Dislikes, -1);

            await _blog.UpdateOneAsync(b => b.Id == objectId, update);

            var updatedBlog = await _blog.Find(b => b.Id == objectId).FirstOrDefaultAsync();
            return MapToLikeDislikeResponseDto(updatedBlog, userId);
        }

        public async Task<LikeDislikeResponseDto> LikeCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            // Verify blog exists and contains the comment
            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            if (!blog.Comments.Contains(commentObjectId))
                throw new KeyNotFoundException("Comment not found in this blog");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();
            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            var updateBuilder = Builders<Comment>.Update;
            UpdateDefinition<Comment> update;

            // Toggle like
            if (comment.UserIdsLikes.Contains(userId))
            {
                update = updateBuilder.Combine(
                    updateBuilder.Pull(c => c.UserIdsLikes, userId),
                    updateBuilder.Inc(c => c.Likes, -1)
                );
            }
            else
            {
                var updates = new List<UpdateDefinition<Comment>>();

                // Remove dislike if exists
                if (comment.UserIdsDislikes.Contains(userId))
                {
                    updates.Add(updateBuilder.Pull(c => c.UserIdsDislikes, userId));
                    updates.Add(updateBuilder.Inc(c => c.Dislikes, -1));
                }

                // Add like
                updates.Add(updateBuilder.Push(c => c.UserIdsLikes, userId));
                updates.Add(updateBuilder.Inc(c => c.Likes, 1));

                update = updateBuilder.Combine(updates);
            }

            // ✅ Update the COMMENT collection, not the blog
            await _comment.UpdateOneAsync(c => c.Id == commentObjectId, update);

            var updatedComment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();

            return new LikeDislikeResponseDto
            {
                LikesCount = updatedComment.Likes,
                DislikesCount = updatedComment.Dislikes,
                UserLiked = updatedComment.UserIdsLikes.Contains(userId),
                UserDisliked = updatedComment.UserIdsDislikes.Contains(userId)
            };
        }

        public async Task<LikeDislikeResponseDto> DislikeCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            ObjectId? com = blog.Comments.FirstOrDefault(c => c == commentObjectId);
            if (com == null)
                throw new KeyNotFoundException("Comment not found");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();

            if (comment.UserIdsDislikes.Contains(userId))
            {
                comment.UserIdsDislikes.Remove(userId);
                comment.Dislikes--;
            }
            else
            {
                if (comment.UserIdsLikes.Contains(userId))
                {
                    comment.UserIdsLikes.Remove(userId);
                    comment.Likes--;
                }

                comment.UserIdsDislikes.Add(userId);
                comment.Dislikes++;
            }

            var update = Builders<Models.Blog.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserIdsLikes.Contains(userId),
                UserDisliked = comment.UserIdsDislikes.Contains(userId)
            };
        }
        
        public async Task<LikeDislikeResponseDto> RemoveLikeFromCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            ObjectId? com = blog.Comments.FirstOrDefault(c => c == commentObjectId);
            if (com == null)
                throw new KeyNotFoundException("Comment not found");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();

            if (comment.UserIdsLikes.Contains(userId))
            {
                comment.UserIdsLikes.Remove(userId);
                comment.Likes--;
            }

            var update = Builders<Models.Blog.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserIdsLikes.Contains(userId),
                UserDisliked = comment.UserIdsDislikes.Contains(userId)
            };
        }
        
        public async Task<LikeDislikeResponseDto> RemoveDislikeFromCommentAsync(string blogId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(blogId, out var blogObjectId) || !ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid blog or comment ID");

            var blog = await _blog.Find(b => b.Id == blogObjectId).FirstOrDefaultAsync();
            if (blog == null)
                throw new KeyNotFoundException("Blog not found");

            ObjectId? com = blog.Comments.FirstOrDefault(c => c == commentObjectId);
            if (com == null)
                throw new KeyNotFoundException("Comment not found");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();

            if (comment.UserIdsDislikes.Contains(userId))
            {
                comment.UserIdsDislikes.Remove(userId);
                comment.Dislikes--;
            }

            var update = Builders<Models.Blog.Blog>.Update.Set(b => b.Comments, blog.Comments);
            await _blog.UpdateOneAsync(b => b.Id == blogObjectId, update);

            return new LikeDislikeResponseDto
            {
                LikesCount = comment.Likes,
                DislikesCount = comment.Dislikes,
                UserLiked = comment.UserIdsLikes.Contains(userId),
                UserDisliked = comment.UserIdsDislikes.Contains(userId)
            };
        }

        //========================  Replies Methods  ====================================
        public async Task<Comment> CreateReplyAsync(string commentId, string userId, CreateCommentDTO dto)
        {
            if (!ObjectId.TryParse(commentId, out var objectId))
                throw new ArgumentException("Invalid blog ID");

            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new ArgumentException("Comment content cannot be empty");

            var comment = await _comment.Find(c => c.Id == objectId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            var reply = new Comment
            {
                Content = dto.Content,
                CommenterId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Tags = dto.Tags ?? new List<string>(),
                Likes = 0,
                Dislikes = 0,
                UserIdsLikes = new List<string>(),
                UserIdsDislikes = new List<string>(),
                Replies = new List<Comment>()
            };

            comment.Replies.Add(reply);
            var update = Builders<Comment>.Update.Set(c => c, comment);
            await _comment.UpdateOneAsync(b => b.Id == objectId, update);

            return reply;
        }

        public async Task<List<Comment>?> GetRepliesAsync(string commentId)
        {
            if (!ObjectId.TryParse(commentId, out var id))
                throw new ArgumentException("Invalid comment id");

            var comment = await _comment.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            return comment.Replies;
        }

        public async Task<Comment> GetReplyById(string replyId, string commentId)
        {
            if (!ObjectId.TryParse(commentId, out var comId))
                throw new ArgumentException("Invalid comment id");
            if (!ObjectId.TryParse(replyId, out var id))
                throw new ArgumentException("Invalid reply id");

            var comment = await _comment.Find(c => c.Id == comId).FirstAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            var reply = comment.Replies.FirstOrDefault(r => r.Id == id);
            if (reply is null)
                throw new KeyNotFoundException("Reply not found");

            return reply;
        }

        public async Task<Comment> UpdateReply(string commentId, string replyId, UpdateReplyDTO updateDTO, string userId)
        {
            if (!ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid comment ID");
            if (!ObjectId.TryParse(replyId, out var id))
                throw new ArgumentException("Invalid reply id");

            if (string.IsNullOrWhiteSpace(updateDTO.Content))
                throw new ArgumentException("Content cannot be empty");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");

            // ✅ Find once and reuse
            var reply = comment.Replies.FirstOrDefault(r => r.Id == id);
            if (reply is null)
                throw new KeyNotFoundException("Reply not found");

            if (reply.CommenterId != userId)
                throw new UnauthorizedAccessException("You can only update your own reply");

            reply.Content = updateDTO.Content;
            reply.UpdatedAt = DateTime.UtcNow;

            if (updateDTO.Tags is not null && updateDTO.Tags.Count > 0)  // ✅ Fixed logic
                reply.Tags = updateDTO.Tags;

            var update = Builders<Comment>.Update.Set(c => c.Replies, comment.Replies);
            await _comment.UpdateOneAsync(c => c.Id == commentObjectId, update);

            return reply;
        }

        public async Task<bool> DeleteReply(string replyId, string commentId, string userId)
        {
            if (!ObjectId.TryParse(commentId, out var commentObjectId))
                throw new ArgumentException("Invalid comment ID");
            if (!ObjectId.TryParse(replyId, out var id))
                throw new ArgumentException("Invalid reply id");

            var comment = await _comment.Find(c => c.Id == commentObjectId).FirstOrDefaultAsync();
            if (comment is null)
                throw new KeyNotFoundException("Comment not found");
            var reply = comment.Replies.Find(r => r.Id == id);
            if (reply is null)
                throw new KeyNotFoundException("Reply not found");

            if (reply.CommenterId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reply");

            comment.Replies.Remove(reply);

            var update = Builders<Comment>.Update.Set(c => c, comment);
            await _comment.UpdateOneAsync(b => b.Id == commentObjectId, update);

            return true;
        }
        


        //========================  Helper Methods  =====================================

        private List<string> NormalizeTags(List<string> tags)
        {
            return tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().StartsWith("#") ? t.Trim() : $"#{t.Trim()}")
                .Distinct()
                .ToList();
        }

        private LikeDislikeResponseDto MapToLikeDislikeResponseDto(Models.Blog.Blog blog, string userId)
        {
            return new LikeDislikeResponseDto
            {
                LikesCount = blog.Likes,
                DislikesCount = blog.Dislikes,
                UserLiked = blog.UserIdsLikes.Contains(userId),
                UserDisliked = blog.UserIdsDislikes.Contains(userId)
            };
        }

    }
}
