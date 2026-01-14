using RestService.Domain.Entities;

namespace RestService.Domain.Interfaces;

public interface IFileRepository
{
    Task<FileMetadata?> GetByFileIdAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FileMetadata>> GetSharedWithUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(FileMetadata metadata, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public interface IMinioService
{
    Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string objectKey, CancellationToken cancellationToken = default);
}
