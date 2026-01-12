using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Uploads the processed file to MinIO object storage
/// </summary>
public class MinioUploadFilter : IFilter
{
    private readonly IMinioService _minioService;
    private readonly ILogger<MinioUploadFilter> _logger;

    public string Name => "MinioUploadFilter";
    public int Order => 4;

    public MinioUploadFilter(IMinioService minioService, ILogger<MinioUploadFilter> logger)
    {
        _minioService = minioService;
        _logger = logger;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading file to MinIO: {FileName}", context.OriginalFileName);

        // Use encrypted file if available, otherwise use original
        var fileToUpload = context.IsEncrypted && !string.IsNullOrEmpty(context.EncryptedFilePath)
            ? context.EncryptedFilePath
            : context.TempFilePath;

        // Generate object key with folder structure
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var objectKey = $"{context.UserId}/{timestamp}/{context.FileId}/{context.OriginalFileName}";
        
        if (context.IsEncrypted)
        {
            objectKey += ".encrypted";
        }

        try
        {
            await _minioService.UploadFileAsync(fileToUpload, objectKey, context.ContentType, cancellationToken);
            context.MinioObjectKey = objectKey;
            
            _logger.LogInformation("File uploaded to MinIO with key: {ObjectKey}", objectKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to MinIO: {FileName}", context.OriginalFileName);
            context.Errors.Add($"MinIO upload failed: {ex.Message}");
        }

        return context;
    }
}
