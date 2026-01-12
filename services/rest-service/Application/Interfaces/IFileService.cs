using RestService.Domain.Entities;
using RestService.DTOs;

namespace RestService.Application.Services;

public interface IFileService
{
    Task<FileDto?> GetFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
    Task<FileListResponse> GetUserFilesAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FileDownloadResponse> GetDownloadUrlAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileDto>> GetSharedFilesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ShareResponse> ShareFileAsync(Guid fileId, Guid ownerUserId, ShareFileRequest request, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
}
