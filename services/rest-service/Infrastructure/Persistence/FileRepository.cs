using MongoDB.Driver;
using RestService.Domain.Entities;
using RestService.Domain.Interfaces;

namespace RestService.Infrastructure.Persistence;

public class FileRepository : IFileRepository
{
    private readonly IMongoCollection<FileMetadata> _collection;
    private readonly ILogger<FileRepository> _logger;

    public FileRepository(IMongoDatabase database, ILogger<FileRepository> logger)
    {
        _collection = database.GetCollection<FileMetadata>("file_metadata");
        _logger = logger;
    }

    public async Task<FileMetadata?> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.FileId == fileId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(x => x.UserId == userId)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return (int)await _collection.CountDocumentsAsync(x => x.UserId == userId, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(_ => true)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        return (int)await _collection.CountDocumentsAsync(_ => true, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<FileMetadata>> GetSharedWithUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<FileMetadata>.Filter.ElemMatch(
            x => x.Shares,
            s => s.SharedWithUserId == userId && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow));

        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(FileMetadata metadata, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == metadata.Id, metadata, cancellationToken: cancellationToken);
        _logger.LogInformation("Updated file metadata: {FileId}", metadata.FileId);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        _logger.LogInformation("Deleted file metadata: {Id}", id);
    }
}
