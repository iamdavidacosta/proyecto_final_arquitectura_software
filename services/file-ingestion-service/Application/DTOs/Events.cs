namespace FileIngestionService.Application.DTOs;

public record FileUploadedEvent(
    Guid FileId,
    Guid CorrelationId,
    Guid UserId,
    string UserEmail,
    string FileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAt,
    string Status,
    string StoragePath
);
