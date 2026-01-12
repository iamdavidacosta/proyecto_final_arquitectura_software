using Minio;
using Minio.DataModel.Args;
using SoapService.Domain.Interfaces;

namespace SoapService.Infrastructure.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioService> logger)
    {
        _minioClient = minioClient;
        _bucketName = configuration["MinIO:BucketName"] ?? "fileshare-bucket";
        _logger = logger;
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file from MinIO: {ObjectKey}", objectKey);

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey), cancellationToken);

        _logger.LogInformation("File deleted from MinIO: {ObjectKey}", objectKey);
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithExpiry(expiryInSeconds));

        _logger.LogInformation("Generated presigned URL for: {ObjectKey}, Expiry: {ExpirySeconds}s", objectKey, expiryInSeconds);
        return url;
    }
}
