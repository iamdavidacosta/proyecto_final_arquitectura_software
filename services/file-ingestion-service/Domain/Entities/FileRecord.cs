namespace FileIngestionService.Domain.Entities;

public class FileRecord
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string OriginalFileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string Status { get; private set; } = "Received";
    public string? Sha256Hash { get; private set; }
    public string? OriginalMinioPath { get; private set; }
    public string? EncryptedMinioPath { get; private set; }
    public string? Description { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private FileRecord() { } // EF Core

    public static FileRecord Create(
        Guid userId,
        string fileName,
        string originalFileName,
        string contentType,
        long fileSize,
        string? description = null)
    {
        return new FileRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CorrelationId = Guid.NewGuid(),
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            Status = "Received",
            Description = description,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
        if (status == "UploadedToMinIO" || status == "Failed")
        {
            ProcessedAt = DateTime.UtcNow;
        }
    }

    public void SetHash(string hash)
    {
        Sha256Hash = hash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMinioPaths(string originalPath, string encryptedPath)
    {
        OriginalMinioPath = originalPath;
        EncryptedMinioPath = encryptedPath;
        UpdatedAt = DateTime.UtcNow;
    }
}
