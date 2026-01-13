namespace FileProcessingPipeline.Domain.Interfaces;

public interface IMinioService
{
    Task<string> UploadFileAsync(string filePath, string objectKey, string contentType, CancellationToken cancellationToken = default, string? bucketName = null);
    Task<Stream> DownloadFileAsync(string objectKey, CancellationToken cancellationToken = default, string? bucketName = null);
    Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default, string? bucketName = null);
    Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default, string? bucketName = null);
    Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default, string? bucketName = null);
}
