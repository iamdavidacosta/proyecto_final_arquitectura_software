using MongoDB.Driver;
using SoapService.Domain.Entities;
using SoapService.Domain.Interfaces;

namespace SoapService.Infrastructure.Persistence;

public class FileMetadataRepository : IFileMetadataRepository
{
    private readonly IMongoCollection<FileMetadata> _collection;
    private readonly ILogger<FileMetadataRepository> _logger;

    public FileMetadataRepository(IMongoDatabase database, ILogger<FileMetadataRepository> logger)
    {
        _collection = database.GetCollection<FileMetadata>("file_metadata");
        _logger = logger;
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

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        _logger.LogInformation("Deleted file metadata with Id: {Id}", id);
    }
}
