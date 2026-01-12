using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Stores file metadata in MongoDB
/// </summary>
public class MongoStorageFilter : IFilter
{
    private readonly IFileMetadataRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MongoStorageFilter> _logger;

    public string Name => "MongoStorageFilter";
    public int Order => 5;

    public MongoStorageFilter(
        IFileMetadataRepository repository,
        IConfiguration configuration,
        ILogger<MongoStorageFilter> logger)
    {
        _repository = repository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Storing metadata in MongoDB for file: {FileName}", context.OriginalFileName);

        var metadata = new FileMetadata
        {
            FileId = context.FileId,
            UserId = context.UserId,
            OriginalFileName = context.OriginalFileName,
            StoredFileName = Path.GetFileName(context.MinioObjectKey ?? context.OriginalFileName),
            ContentType = context.ContentType,
            FileSize = context.OriginalFileSize,
            Hash = context.Hash ?? string.Empty,
            HashAlgorithm = context.HashAlgorithm,
            MinioObjectKey = context.MinioObjectKey ?? string.Empty,
            MinioBucket = _configuration["MinIO:BucketName"] ?? "fileshare-bucket",
            IsEncrypted = context.IsEncrypted,
            Description = context.Description,
            Status = context.HasErrors ? FileProcessingStatus.Failed : FileProcessingStatus.Completed,
            ErrorMessage = context.HasErrors ? string.Join("; ", context.Errors) : null,
            ProcessedAt = DateTime.UtcNow,
            CustomMetadata = context.ExtractedMetadata
        };

        try
        {
            await _repository.CreateAsync(metadata, cancellationToken);
            _logger.LogInformation("Metadata stored in MongoDB with ID: {Id}", metadata.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store metadata in MongoDB for file: {FileName}", context.OriginalFileName);
            context.Errors.Add($"MongoDB storage failed: {ex.Message}");
        }

        return context;
    }
}
