using SoapService.Contracts;
using SoapService.Domain.Interfaces;

namespace SoapService.Services;

public class FileShareService : IFileShareService
{
    private readonly IFileMetadataRepository _repository;
    private readonly IMinioService _minioService;
    private readonly ILogger<FileShareService> _logger;

    public FileShareService(
        IFileMetadataRepository repository,
        IMinioService minioService,
        ILogger<FileShareService> logger)
    {
        _repository = repository;
        _minioService = minioService;
        _logger = logger;
    }

    public async Task<GetFileResponse> GetFileAsync(GetFileRequest request)
    {
        _logger.LogInformation("SOAP GetFile request for FileId: {FileId}", request.FileId);

        try
        {
            if (!Guid.TryParse(request.FileId, out var fileId))
            {
                return new GetFileResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid FileId format"
                };
            }

            var metadata = await _repository.GetByFileIdAsync(fileId);
            if (metadata == null)
            {
                return new GetFileResponse
                {
                    Success = false,
                    ErrorMessage = "File not found"
                };
            }

            return new GetFileResponse
            {
                Success = true,
                File = MapToFileInfo(metadata)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFile for FileId: {FileId}", request.FileId);
            return new GetFileResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while retrieving the file"
            };
        }
    }

    public async Task<GetUserFilesResponse> GetUserFilesAsync(GetUserFilesRequest request)
    {
        _logger.LogInformation("SOAP GetUserFiles request for UserId: {UserId}", request.UserId);

        try
        {
            if (!Guid.TryParse(request.UserId, out var userId))
            {
                return new GetUserFilesResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid UserId format"
                };
            }

            var files = await _repository.GetByUserIdAsync(userId);

            return new GetUserFilesResponse
            {
                Success = true,
                Files = files.Select(MapToFileInfo).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserFiles for UserId: {UserId}", request.UserId);
            return new GetUserFilesResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while retrieving files"
            };
        }
    }

    public async Task<GetDownloadUrlResponse> GetDownloadUrlAsync(GetDownloadUrlRequest request)
    {
        _logger.LogInformation("SOAP GetDownloadUrl request for FileId: {FileId}", request.FileId);

        try
        {
            if (!Guid.TryParse(request.FileId, out var fileId))
            {
                return new GetDownloadUrlResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid FileId format"
                };
            }

            var metadata = await _repository.GetByFileIdAsync(fileId);
            if (metadata == null)
            {
                return new GetDownloadUrlResponse
                {
                    Success = false,
                    ErrorMessage = "File not found"
                };
            }

            var expirySeconds = request.ExpiryInSeconds > 0 ? request.ExpiryInSeconds : 3600;
            var url = await _minioService.GetPresignedUrlAsync(metadata.MinioObjectKey, expirySeconds);

            return new GetDownloadUrlResponse
            {
                Success = true,
                DownloadUrl = url,
                ExpiresAt = DateTime.UtcNow.AddSeconds(expirySeconds)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDownloadUrl for FileId: {FileId}", request.FileId);
            return new GetDownloadUrlResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while generating download URL"
            };
        }
    }

    public async Task<DeleteFileResponse> DeleteFileAsync(DeleteFileRequest request)
    {
        _logger.LogInformation("SOAP DeleteFile request for FileId: {FileId}, UserId: {UserId}", 
            request.FileId, request.UserId);

        try
        {
            if (!Guid.TryParse(request.FileId, out var fileId) || !Guid.TryParse(request.UserId, out var userId))
            {
                return new DeleteFileResponse
                {
                    Success = false,
                    ErrorMessage = "Invalid FileId or UserId format"
                };
            }

            var metadata = await _repository.GetByFileIdAsync(fileId);
            if (metadata == null)
            {
                return new DeleteFileResponse
                {
                    Success = false,
                    ErrorMessage = "File not found"
                };
            }

            if (metadata.UserId != userId)
            {
                return new DeleteFileResponse
                {
                    Success = false,
                    ErrorMessage = "Unauthorized to delete this file"
                };
            }

            // Delete from MinIO
            await _minioService.DeleteFileAsync(metadata.MinioObjectKey);

            // Delete metadata
            await _repository.DeleteAsync(metadata.Id);

            _logger.LogInformation("File deleted successfully: {FileId}", request.FileId);

            return new DeleteFileResponse { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteFile for FileId: {FileId}", request.FileId);
            return new DeleteFileResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while deleting the file"
            };
        }
    }

    private static Contracts.FileInfo MapToFileInfo(Domain.Entities.FileMetadata metadata)
    {
        return new Contracts.FileInfo
        {
            FileId = metadata.FileId.ToString(),
            UserId = metadata.UserId.ToString(),
            FileName = metadata.OriginalFileName,
            ContentType = metadata.ContentType,
            FileSize = metadata.FileSize,
            Hash = metadata.Hash,
            IsEncrypted = metadata.IsEncrypted,
            Description = metadata.Description,
            Status = metadata.Status.ToString(),
            CreatedAt = metadata.CreatedAt,
            ProcessedAt = metadata.ProcessedAt
        };
    }
}
