using FileIngestionService.Application.DTOs;
using FileIngestionService.Application.Interfaces;
using FileIngestionService.Domain.Entities;
using FileIngestionService.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileIngestionService.Application.Services;

public class FileIngestionServiceImpl : IFileIngestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileIngestionServiceImpl> _logger;

    public FileIngestionServiceImpl(
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        IConfiguration configuration,
        ILogger<FileIngestionServiceImpl> logger)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<FileUploadResponse> UploadFileAsync(
        Guid userId, 
        string userEmail, 
        IFormFile file, 
        string? description,
        Func<FileUploadProgress, Task>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting file upload for user {UserId}, file: {FileName}", userId, file.FileName);

        // Validate file
        var maxSize = _configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 104857600);
        if (file.Length > maxSize)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {maxSize} bytes");
        }

        // Create file record
        var fileRecord = FileRecord.Create(
            userId,
            $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}",
            file.FileName,
            file.ContentType,
            file.Length,
            description
        );

        await _unitOfWork.Files.AddAsync(fileRecord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Report initial progress
        if (progressCallback != null)
        {
            await progressCallback(new FileUploadProgress(fileRecord.Id, 10, "Received", "File record created"));
        }

        // Save file to disk
        var uploadPath = _configuration["FileUpload:UploadPath"] ?? "/app/uploads";
        Directory.CreateDirectory(uploadPath);
        var filePath = Path.Combine(uploadPath, fileRecord.FileName);

        try
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            var totalBytes = file.Length;
            var bytesWritten = 0L;
            var buffer = new byte[81920];
            int bytesRead;

            using var sourceStream = file.OpenReadStream();
            while ((bytesRead = await sourceStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                bytesWritten += bytesRead;

                if (progressCallback != null)
                {
                    var percent = (int)((bytesWritten * 80 / totalBytes) + 10); // 10-90%
                    await progressCallback(new FileUploadProgress(fileRecord.Id, percent, "Uploading", $"{bytesWritten}/{totalBytes} bytes"));
                }
            }

            _logger.LogInformation("File saved to disk: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file to disk");
            fileRecord.UpdateStatus("Failed");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }

        // Report progress
        if (progressCallback != null)
        {
            await progressCallback(new FileUploadProgress(fileRecord.Id, 95, "Publishing", "Publishing event to queue"));
        }

        // Publish event to RabbitMQ
        var @event = new FileUploadedEvent(
            fileRecord.Id,
            fileRecord.CorrelationId,
            fileRecord.UserId,
            userEmail,
            fileRecord.OriginalFileName,
            fileRecord.ContentType,
            fileRecord.FileSize,
            fileRecord.UploadedAt,
            fileRecord.Status,
            filePath
        );

        await _eventPublisher.PublishFileUploadedAsync(@event, cancellationToken);

        _logger.LogInformation("File uploaded successfully: {FileId}", fileRecord.Id);

        // Report completion
        if (progressCallback != null)
        {
            await progressCallback(new FileUploadProgress(fileRecord.Id, 100, "Completed", "File uploaded and queued for processing"));
        }

        return new FileUploadResponse(
            fileRecord.Id,
            fileRecord.CorrelationId,
            fileRecord.Status,
            "File uploaded successfully and queued for processing"
        );
    }

    public async Task<IEnumerable<FileInfoDto>> GetUserFilesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var files = await _unitOfWork.Files.GetByUserIdAsync(userId, cancellationToken);
        return files.Select(f => new FileInfoDto(
            f.Id,
            f.OriginalFileName,
            f.ContentType,
            f.FileSize,
            f.Status,
            f.UploadedAt
        ));
    }

    public async Task<FileInfoDto?> GetFileByIdAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var file = await _unitOfWork.Files.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.UserId != userId)
        {
            return null;
        }

        return new FileInfoDto(
            file.Id,
            file.OriginalFileName,
            file.ContentType,
            file.FileSize,
            file.Status,
            file.UploadedAt
        );
    }

    public async Task<FileInfoDto?> GetFileInfoAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _unitOfWork.Files.GetByIdAsync(fileId, cancellationToken);
        if (file == null)
        {
            return null;
        }

        return new FileInfoDto(
            file.Id,
            file.OriginalFileName,
            file.ContentType,
            file.FileSize,
            file.Status,
            file.UploadedAt
        );
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, Guid userId, CancellationToken cancellationToken = default)
    {
        var file = await _unitOfWork.Files.GetByIdAsync(fileId, cancellationToken);
        if (file == null || file.UserId != userId)
        {
            return false;
        }

        file.UpdateStatus("Deleted");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("File marked as deleted: {FileId}", fileId);
        return true;
    }
}
