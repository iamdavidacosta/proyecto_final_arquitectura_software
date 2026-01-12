namespace RestService.Domain.Entities;

public class FileMetadata
{
    public string Id { get; set; } = string.Empty;
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "SHA256";
    public string MinioObjectKey { get; set; } = string.Empty;
    public string MinioBucket { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public string? Description { get; set; }
    public FileProcessingStatus Status { get; set; } = FileProcessingStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
    public List<FileShare> Shares { get; set; } = new();
}

public class FileShare
{
    public Guid ShareId { get; set; } = Guid.NewGuid();
    public Guid? SharedWithUserId { get; set; }
    public string? SharedWithEmail { get; set; }
    public SharePermission Permission { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
}

public enum FileProcessingStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum SharePermission
{
    Read,
    ReadWrite
}
