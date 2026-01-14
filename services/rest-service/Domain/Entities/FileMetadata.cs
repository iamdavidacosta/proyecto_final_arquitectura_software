using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RestService.Domain.Entities;

public class FileMetadata
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("fileId")]
    [BsonRepresentation(BsonType.String)]
    public Guid FileId { get; set; }
    
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }
    
    [BsonElement("originalFileName")]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [BsonElement("storedFileName")]
    public string StoredFileName { get; set; } = string.Empty;
    
    [BsonElement("contentType")]
    public string ContentType { get; set; } = string.Empty;
    
    [BsonElement("fileSize")]
    public long FileSize { get; set; }
    
    [BsonElement("hash")]
    public string Hash { get; set; } = string.Empty;
    
    [BsonElement("hashAlgorithm")]
    public string HashAlgorithm { get; set; } = "SHA256";
    
    [BsonElement("minioObjectKey")]
    public string MinioObjectKey { get; set; } = string.Empty;
    
    [BsonElement("minioOriginalObjectKey")]
    public string MinioOriginalObjectKey { get; set; } = string.Empty;
    
    [BsonElement("minioBucket")]
    public string MinioBucket { get; set; } = string.Empty;
    
    [BsonElement("originalBucket")]
    public string OriginalBucket { get; set; } = string.Empty;
    
    [BsonElement("isEncrypted")]
    public bool IsEncrypted { get; set; }
    
    [BsonElement("isDecryptionValidated")]
    public bool IsDecryptionValidated { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("status")]
    public FileProcessingStatus Status { get; set; } = FileProcessingStatus.Pending;
    
    [BsonElement("errorMessage")]
    public string? ErrorMessage { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }
    
    [BsonElement("customMetadata")]
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
    
    [BsonElement("shares")]
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
