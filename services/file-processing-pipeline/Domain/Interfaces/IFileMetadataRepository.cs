using FileProcessingPipeline.Domain.Entities;

namespace FileProcessingPipeline.Domain.Interfaces;

public interface IFileMetadataRepository
{
    Task<FileMetadata?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<FileMetadata?> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FileMetadata> CreateAsync(FileMetadata metadata, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileMetadata metadata, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
