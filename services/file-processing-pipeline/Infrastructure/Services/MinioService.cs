using FileProcessingPipeline.Domain.Interfaces;
using Minio;
using Minio.DataModel.Args;

namespace FileProcessingPipeline.Infrastructure.Services;

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

    public async Task<string> UploadFileAsync(string filePath, string objectKey, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading file to MinIO: {ObjectKey}", objectKey);

        // Ensure bucket exists
        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucketName), cancellationToken);
        
        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_bucketName), cancellationToken);
            _logger.LogInformation("Created bucket: {BucketName}", _bucketName);
        }

        await using var fileStream = File.OpenRead(filePath);
        
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType), cancellationToken);

        _logger.LogInformation("File uploaded to MinIO: {BucketName}/{ObjectKey}", _bucketName, objectKey);
        return objectKey;
    }

    public async Task<Stream> DownloadFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading file from MinIO: {ObjectKey}", objectKey);

        var memoryStream = new MemoryStream();
        
        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream)), cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    public async Task DeleteFileAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file from MinIO: {ObjectKey}", objectKey);

        await _minioClient.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey), cancellationToken);

        _logger.LogInformation("File deleted from MinIO: {ObjectKey}", objectKey);
    }

    public async Task<bool> FileExistsAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectKey), cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> GetPresignedUrlAsync(string objectKey, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        var url = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(objectKey)
            .WithExpiry(expiryInSeconds));

        return url;
    }
}
