namespace FileProcessingPipeline.Domain.Interfaces;

public interface IMinioService
{
    Task<string> UploadFileAsync(string filePath, string objectKey, string contentType, CancellationToken cancellationToken = default);
    Task<Stream> DownloadFileAsync(string objectKey, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default);
    Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default);
}
