using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Extracts metadata from the file based on its type
/// </summary>
public class MetadataFilter : IFilter
{
    private readonly ILogger<MetadataFilter> _logger;

    public string Name => "MetadataFilter";
    public int Order => 2;

    public MetadataFilter(ILogger<MetadataFilter> logger)
    {
        _logger = logger;
    }

    public Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting metadata for file: {FileName}", context.OriginalFileName);

        // Extract basic metadata
        var fileInfo = new FileInfo(context.TempFilePath);
        
        context.ExtractedMetadata["extension"] = Path.GetExtension(context.OriginalFileName).ToLowerInvariant();
        context.ExtractedMetadata["originalSize"] = context.OriginalFileSize;
        context.ExtractedMetadata["currentSize"] = fileInfo.Length;
        context.ExtractedMetadata["createdAt"] = fileInfo.CreationTimeUtc;
        context.ExtractedMetadata["lastModified"] = fileInfo.LastWriteTimeUtc;

        // Add content type specific metadata
        var extension = Path.GetExtension(context.OriginalFileName).ToLowerInvariant();
        context.ExtractedMetadata["fileCategory"] = GetFileCategory(extension);

        // Add MIME type mapping
        context.ExtractedMetadata["mimeType"] = context.ContentType;

        _logger.LogInformation("Metadata extracted: {MetadataCount} properties", context.ExtractedMetadata.Count);

        return Task.FromResult(context);
    }

    private static string GetFileCategory(string extension)
    {
        return extension switch
        {
            ".pdf" => "document",
            ".doc" or ".docx" => "document",
            ".xls" or ".xlsx" => "spreadsheet",
            ".ppt" or ".pptx" => "presentation",
            ".txt" or ".md" => "text",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "image",
            ".mp4" or ".avi" or ".mov" or ".mkv" => "video",
            ".mp3" or ".wav" or ".flac" or ".ogg" => "audio",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "archive",
            ".json" or ".xml" or ".yaml" or ".yml" => "data",
            ".cs" or ".java" or ".py" or ".js" or ".ts" => "code",
            _ => "other"
        };
    }
}
