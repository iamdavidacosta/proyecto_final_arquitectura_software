using FileProcessingPipeline.Domain.Interfaces;
using Minio;
using Minio.DataModel.Args;

namespace FileProcessingPipeline.Infrastructure.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _defaultBucketName;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioService> logger)
    {
        _minioClient = minioClient;
        _defaultBucketName = configuration["MinIO:BucketName"] ?? "fileshare-bucket";
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(string filePath, string objectKey, string contentType, CancellationToken cancellationToken = default, string? bucketName = null)
    {
        var bucket = bucketName ?? _defaultBucketName;
        _logger.LogInformation("Uploading file to MinIO: {Bucket}/{ObjectKey}", bucket, objectKey);

        // Ensure bucket exists
        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);
        
        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket), cancellationToken);
            _logger.LogInformation("Created bucket: {BucketName}", bucket);
        }

        await using var fileStream = File.OpenRead(filePath);
        
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        _logger.LogInformation("File uploaded to MinIO: {BucketName}/{ObjectKey}", bucket, objectKey);
        return objectKey;
    }

    public async Task<Stream> DownloadFileAsync(string objectKey, CancellationToken cancellationToken = default, string? bucketName = null)
    {
        var bucket = bucketName ?? _defaultBucketName;
        _logger.LogInformation("Downloading file from MinIO: {Bucket}/{ObjectKey}", bucket, objectKey);

        var memoryStream = new MemoryStream();
        
        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream)), cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default, string? bucketName = null)
    {
        var bucket = bucketName ?? _defaultBucketName;
        _logger.LogInformation("Deleting file from MinIO: {Bucket}/{ObjectKey}", bucket, objectKey);

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey), cancellationToken);

        _logger.LogInformation("File deleted from MinIO: {Bucket}/{ObjectKey}", bucket, objectKey);
    }

    public async Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default, string? bucketName = null)
    {
        var bucket = bucketName ?? _defaultBucketName;
        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default, string? bucketName = null)
    {
        var bucket = bucketName ?? _defaultBucketName;
        var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithExpiry(expiryInSeconds));

        return url;
    }
}
