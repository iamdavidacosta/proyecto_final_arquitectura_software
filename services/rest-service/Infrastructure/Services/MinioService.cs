using Minio;
using Minio.DataModel.Args;
using RestService.Domain.Interfaces;

namespace RestService.Infrastructure.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _originalBucket;
    private readonly string _encryptedBucket;
    private readonly string _publicEndpoint;
    private readonly string _internalEndpoint;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioService> logger)
    {
        _minioClient = minioClient;
        _originalBucket = configuration["MinIO:OriginalBucket"] ?? "original-files";
        _encryptedBucket = configuration["MinIO:EncryptedBucket"] ?? "encrypted-files";
        _internalEndpoint = configuration["MinIO:Endpoint"] ?? "minio:9000";
        _publicEndpoint = configuration["MinIO:PublicEndpoint"] ?? "localhost:9001";
        _logger = logger;
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file from MinIO: {ObjectKey}", objectKey);

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_originalBucket)
            .WithObject(objectKey), cancellationToken);

        _logger.LogInformation("File deleted from MinIO: {ObjectKey}", objectKey);
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        return await GetPresignedUrlAsync(objectKey, _originalBucket, expiryInSeconds, cancellationToken);
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, string bucket, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry(expiryInSeconds));

        // Replace internal endpoint with public endpoint for browser access
        var publicUrl = url.Replace(_internalEndpoint, _publicEndpoint);
        
        _logger.LogInformation("Generated presigned URL for: {Bucket}/{ObjectKey}", bucket, objectKey);
        return publicUrl;
    }

    public async Task<Stream> DownloadFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_originalBucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream)), cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }
}
