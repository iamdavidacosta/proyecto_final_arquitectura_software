using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileProcessingPipeline.Domain.Entities;

public class FileMetadata
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string Hash { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "SHA256";
    public string MinioObjectKey { get; set; } = string.Empty;
    public string MinioOriginalObjectKey { get; set; } = string.Empty;
    public string MinioBucket { get; set; } = string.Empty;
    public string OriginalBucket { get; set; } = string.Empty;
    public bool IsEncrypted { get; set; }
    public bool IsDecryptionValidated { get; set; }
    public string? Description { get; set; }
    public FileProcessingStatus Status { get; set; } = FileProcessingStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
}

public enum FileProcessingStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
