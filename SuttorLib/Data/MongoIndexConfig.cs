using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SuttorLib.Models;

namespace SuttorLib.Data
{
    public class MongoIndexConfig
    {
        private readonly IOptions<BlogDbSettings> _dbSettings;

        public MongoIndexConfig(IOptions<BlogDbSettings> dbSettings)
        {
            _dbSettings = dbSettings;
        }

        public async Task CreateIndexesAsync()
        {
            try
            {
                var mongoClient = new MongoClient(_dbSettings?.Value?.ConnectionString);
                var mongoDatabase = mongoClient.GetDatabase(_dbSettings?.Value?.DatabaseName);

                // Blog collection indexes
                var blogCollection = mongoDatabase.GetCollection<Blog>(_dbSettings?.Value?.BlogCollection);

                var blogIndexModels = new CreateIndexModel<Blog>[]
                {
                    new CreateIndexModel<Blog>(Builders<Blog>.IndexKeys.Ascending(b => b.Title)),
                    new CreateIndexModel<Blog>(Builders<Blog>.IndexKeys.Ascending(b => b.Content)),
                    new CreateIndexModel<Blog>(Builders<Blog>.IndexKeys.Ascending(b => b.Tags)),
                    new CreateIndexModel<Blog>(Builders<Blog>.IndexKeys.Ascending(b => b.Views)),
                    new CreateIndexModel<Blog>(Builders<Blog>.IndexKeys.Ascending(b => b.PublisherId))
                };

                await blogCollection.Indexes.CreateManyAsync(blogIndexModels);

                // Comment collection indexes
                var commentCollection = mongoDatabase.GetCollection<Comment>(_dbSettings?.Value?.CommentCollection);

                var commentIndexModels = new CreateIndexModel<Comment>[]
                {
                    new CreateIndexModel<Comment>(Builders<Comment>.IndexKeys.Ascending(c => c.Content)),
                    new CreateIndexModel<Comment>(Builders<Comment>.IndexKeys.Ascending(c => c.CommenterId)),
                    new CreateIndexModel<Comment>(Builders<Comment>.IndexKeys.Ascending(c => c.Tags))
                };

                await commentCollection.Indexes.CreateManyAsync(commentIndexModels);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create MongoDB indexes.", ex);
            }
        }
    }
}