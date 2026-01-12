using SoapService.Domain.Entities;

namespace SoapService.Domain.Interfaces;

public interface IFileMetadataRepository
{
    Task<FileMetadata?> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public interface IMinioService
{
    Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
}
