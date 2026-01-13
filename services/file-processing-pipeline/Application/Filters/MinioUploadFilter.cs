using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Uploads both the original and encrypted files to MinIO object storage
/// </summary>
public class MinioUploadFilter : IFilter
{
    private readonly IMinioService _minioService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MinioUploadFilter> _logger;

    public string Name => "MinioUploadFilter";
    public int Order => 5; // After decryption validation (4)

    public MinioUploadFilter(
        IMinioService minioService, 
        IConfiguration configuration,
        ILogger<MinioUploadFilter> logger)
    {
        _minioService = minioService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading files to MinIO: {FileName}", context.OriginalFileName);

        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var basePath = $"{context.UserId}/{timestamp}/{context.FileId}";
        
        var originalBucket = _configuration["MinIO:OriginalBucket"] ?? "original-files";
        var encryptedBucket = _configuration["MinIO:EncryptedBucket"] ?? "encrypted-files";

        try
        {
            // Upload original file to original-files bucket
            var originalObjectKey = $"{basePath}/{context.OriginalFileName}";
            await _minioService.UploadFileAsync(
                context.TempFilePath, 
                originalObjectKey, 
                context.ContentType, 
                cancellationToken,
                originalBucket);
            context.MinioOriginalObjectKey = originalObjectKey;
            
            _logger.LogInformation("Original file uploaded to MinIO bucket '{Bucket}' with key: {ObjectKey}", 
                originalBucket, originalObjectKey);

            // Upload encrypted file to encrypted-files bucket if available
            if (context.IsEncrypted && !string.IsNullOrEmpty(context.EncryptedFilePath))
            {
                var encryptedObjectKey = $"{basePath}/{context.OriginalFileName}.encrypted";
                await _minioService.UploadFileAsync(
                    context.EncryptedFilePath, 
                    encryptedObjectKey, 
                    "application/octet-stream", 
                    cancellationToken,
                    encryptedBucket);
                context.MinioObjectKey = encryptedObjectKey;
                
                _logger.LogInformation("Encrypted file uploaded to MinIO bucket '{Bucket}' with key: {ObjectKey}", 
                    encryptedBucket, encryptedObjectKey);
            }

            // Store metadata about uploads
            context.ExtractedMetadata["originalBucket"] = originalBucket;
            context.ExtractedMetadata["originalObjectKey"] = originalObjectKey;
            if (context.IsEncrypted)
            {
                context.ExtractedMetadata["encryptedBucket"] = encryptedBucket;
                context.ExtractedMetadata["encryptedObjectKey"] = context.MinioObjectKey!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload files to MinIO: {FileName}", context.OriginalFileName);
            context.Errors.Add($"MinIO upload failed: {ex.Message}");
        }

        return context;
    }
}
