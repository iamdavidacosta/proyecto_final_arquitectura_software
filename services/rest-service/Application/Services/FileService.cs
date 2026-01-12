using RestService.Application.Services;
using RestService.DTOs;
using DomainFileShare = RestService.Domain.Entities.FileShare;
using RestService.Domain.Entities;
using RestService.Domain.Interfaces;

namespace RestService.Application.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _repository;
    private readonly IMinioService _minioService;
    private readonly ILogger<FileService> _logger;

    public FileService(IFileRepository repository, IMinioService minioService, ILogger<FileService> logger)
    {
        _repository = repository;
        _minioService = minioService;
        _logger = logger;
    }

    public async Task<FileDto?> GetFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var file = await _repository.GetByFileIdAsync(fileId, cancellationToken);
        
        if (file == null) return null;
        
        // Check ownership or share permission
        if (file.UserId != userId && !file.Shares.Any(s => s.SharedWithUserId == userId && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow)))
        {
            return null;
        }

        return MapToDto(file);
    }

    public async Task<FileListResponse> GetUserFilesAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var files = await _repository.GetByUserIdAsync(userId, page, pageSize, cancellationToken);
        var totalCount = await _repository.CountByUserIdAsync(userId, cancellationToken);

        return new FileListResponse
        {
            Files = files.Select(MapToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FileDownloadResponse> GetDownloadUrlAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var file = await _repository.GetByFileIdAsync(fileId, cancellationToken);
        
        if (file == null)
            throw new FileNotFoundException("File not found");
        
        // Check ownership or share permission
        if (file.UserId != userId && !file.Shares.Any(s => s.SharedWithUserId == userId && (s.ExpiresAt == null || s.ExpiresAt > DateTime.UtcNow)))
        {
            throw new UnauthorizedAccessException("You don't have permission to download this file");
        }

        var url = await _minioService.GetPresignedUrlAsync(file.MinioObjectKey, 3600, cancellationToken);

        return new FileDownloadResponse
        {
            DownloadUrl = url,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            FileName = file.OriginalFileName
        };
    }

    public async Task<IEnumerable<FileDto>> GetSharedFilesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var files = await _repository.GetSharedWithUserAsync(userId, cancellationToken);
        return files.Select(MapToDto);
    }

    public async Task<ShareResponse> ShareFileAsync(Guid fileId, Guid ownerUserId, ShareFileRequest request, CancellationToken cancellationToken = default)
    {
        var file = await _repository.GetByFileIdAsync(fileId, cancellationToken);
        
        if (file == null)
            throw new FileNotFoundException("File not found");
        
        if (file.UserId != ownerUserId)
            throw new UnauthorizedAccessException("You don't have permission to share this file");

        var share = new DomainFileShare
        {
            ShareId = Guid.NewGuid(),
            SharedWithEmail = request.TargetUserEmail,
            Permission = (Domain.Entities.SharePermission)(int)request.Permission,
            ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(7)
        };

        file.Shares.Add(share);
        await _repository.UpdateAsync(file, cancellationToken);

        _logger.LogInformation("File {FileId} shared with {Email} by user {UserId}", fileId, request.TargetUserEmail, ownerUserId);

        return new ShareResponse
        {
            ShareId = share.ShareId,
            FileId = fileId,
            ShareUrl = $"/api/files/shared/{share.ShareId}",
            ExpiresAt = share.ExpiresAt ?? DateTime.UtcNow.AddDays(7)
        };
    }

    public async Task DeleteFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var file = await _repository.GetByFileIdAsync(fileId, cancellationToken);
        
        if (file == null)
            throw new FileNotFoundException("File not found");
        
        if (file.UserId != userId)
            throw new UnauthorizedAccessException("You don't have permission to delete this file");

        // Delete from MinIO
        await _minioService.DeleteFileAsync(file.MinioObjectKey, cancellationToken);

        // Delete metadata
        await _repository.DeleteAsync(file.Id, cancellationToken);

        _logger.LogInformation("File {FileId} deleted by user {UserId}", fileId, userId);
    }

    private static FileDto MapToDto(FileMetadata file)
    {
        return new FileDto
        {
            FileId = file.FileId,
            UserId = file.UserId,
            FileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            Hash = file.Hash,
            IsEncrypted = file.IsEncrypted,
            Description = file.Description,
            Status = file.Status.ToString(),
            CreatedAt = file.CreatedAt,
            ProcessedAt = file.ProcessedAt
        };
    }
}
