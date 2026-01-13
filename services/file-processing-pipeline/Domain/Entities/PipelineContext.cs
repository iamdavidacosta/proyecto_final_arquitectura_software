namespace FileProcessingPipeline.Domain.Entities;

public class PipelineContext
{
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long OriginalFileSize { get; set; }
    public string? Description { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    
    // Temporary file path for processing
    public string TempFilePath { get; set; } = string.Empty;
    
    // Computed during pipeline
    public string? Hash { get; set; }
    public string HashAlgorithm { get; set; } = "SHA256";
    public bool IsEncrypted { get; set; }
    public bool IsDecryptionValidated { get; set; }
    public string? EncryptedFilePath { get; set; }
    public string? MinioObjectKey { get; set; }
    public string? MinioOriginalObjectKey { get; set; }
    
    // Metadata extracted
    public Dictionary<string, object> ExtractedMetadata { get; set; } = new();
    
    // Error handling
    public List<string> Errors { get; set; } = new();
    public bool HasErrors => Errors.Count > 0;
    
    // Pipeline state
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<string> ProcessedFilters { get; set; } = new();
}
