namespace FileIngestionService.Application.DTOs;

public record FileUploadRequest(
    IFormFile File,
    string? Description
);

public record FileUploadResponse(
    Guid FileId,
    Guid CorrelationId,
    string Status,
    string Message
);

public record FileInfoDto(
    Guid FileId,
    string FileName,
    string ContentType,
    long FileSize,
    string Status,
    DateTime UploadedAt
);

public record FileUploadProgress(
    Guid FileId,
    int PercentComplete,
    string Status,
    string? Message
);
