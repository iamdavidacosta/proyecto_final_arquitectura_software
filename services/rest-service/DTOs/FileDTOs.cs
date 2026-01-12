namespace RestService.DTOs;

public record FileDto
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Hash { get; init; } = string.Empty;
    public bool IsEncrypted { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

public record FileListResponse
{
    public IEnumerable<FileDto> Files { get; init; } = Enumerable.Empty<FileDto>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}

public record FileDownloadResponse
{
    public string DownloadUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string FileName { get; init; } = string.Empty;
}

public record ShareFileRequest
{
    public Guid FileId { get; init; }
    public string TargetUserEmail { get; init; } = string.Empty;
    public SharePermission Permission { get; init; } = SharePermission.Read;
    public DateTime? ExpiresAt { get; init; }
}

public enum SharePermission
{
    Read,
    ReadWrite
}

public record ShareResponse
{
    public Guid ShareId { get; init; }
    public Guid FileId { get; init; }
    public string ShareUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
