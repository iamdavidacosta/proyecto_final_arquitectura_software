using FileIngestionService.Application.DTOs;

namespace FileIngestionService.Application.Interfaces;

public interface IFileIngestionService
{
    Task<FileUploadResponse> UploadFileAsync(Guid userId, string userEmail, IFormFile file, string? description, 
        Func<FileUploadProgress, Task>? progressCallback = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<FileInfoDto>> GetUserFilesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FileInfoDto?> GetFileByIdAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
    Task<FileInfoDto?> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default);
}
