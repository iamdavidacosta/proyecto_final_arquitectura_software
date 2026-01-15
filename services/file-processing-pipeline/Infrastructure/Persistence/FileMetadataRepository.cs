using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;
using MongoDB.Driver;

namespace FileProcessingPipeline.Infrastructure.Persistence;

public class FileMetadataRepository : IFileMetadataRepository
{
    private readonly IMongoCollection<FileMetadata> _collection;
    private readonly ILogger<FileMetadataRepository> _logger;

    public FileMetadataRepository(IMongoDatabase database, ILogger<FileMetadataRepository> logger)
    {
        _collection = database.GetCollection<FileMetadata>("file_metadata");
        _logger = logger;

        // Create indexes (ignore if already exist)
        CreateIndexesSafelyAsync().GetAwaiter().GetResult();
    }

    private async Task CreateIndexesSafelyAsync()
    {
        try
        {
            var indexKeys = Builders<FileMetadata>.IndexKeys;
            
            // Try to create each index individually, ignoring errors for existing indexes
            var indexModels = new[]
            {
                new CreateIndexModel<FileMetadata>(indexKeys.Ascending(x => x.FileId)),
                new CreateIndexModel<FileMetadata>(indexKeys.Ascending(x => x.UserId)),
                new CreateIndexModel<FileMetadata>(indexKeys.Ascending(x => x.Hash)),
                new CreateIndexModel<FileMetadata>(indexKeys.Ascending(x => x.CreatedAt))
            };

            foreach (var index in indexModels)
            {
                try
                {
                    await _collection.Indexes.CreateOneAsync(index);
                }
                catch (MongoCommandException ex) when (ex.Message.Contains("existing index"))
                {
                    // Index already exists with same or different options - ignore
                    _logger.LogDebug("Index already exists, skipping: {Message}", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creating indexes, continuing anyway");
        }
    }

    public async Task<FileMetadata?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<FileMetadata?> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.FileId == fileId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<FileMetadata> CreateAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(metadata, cancellationToken: cancellationToken);
        _logger.LogInformation("Created file metadata with FileId: {FileId}", metadata.FileId);
        return metadata;
    }

    public async Task UpdateAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == metadata.Id, metadata, cancellationToken: cancellationToken);
        _logger.LogInformation("Updated file metadata with FileId: {FileId}", metadata.FileId);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        _logger.LogInformation("Deleted file metadata with Id: {Id}", id);
    }
}
