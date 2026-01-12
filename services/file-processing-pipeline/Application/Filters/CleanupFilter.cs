using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Cleanup filter - removes temporary files after processing
/// </summary>
public class CleanupFilter : IFilter
{
    private readonly ILogger<CleanupFilter> _logger;

    public string Name => "CleanupFilter";
    public int Order => 100; // Always run last

    public CleanupFilter(ILogger<CleanupFilter> logger)
    {
        _logger = logger;
    }

    public Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cleaning up temporary files for FileId: {FileId}", context.FileId);

        // Clean up temp file
        if (!string.IsNullOrEmpty(context.TempFilePath) && File.Exists(context.TempFilePath))
        {
            try
            {
                File.Delete(context.TempFilePath);
                _logger.LogDebug("Deleted temp file: {TempFilePath}", context.TempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temp file: {TempFilePath}", context.TempFilePath);
            }
        }

        // Clean up encrypted file
        if (!string.IsNullOrEmpty(context.EncryptedFilePath) && File.Exists(context.EncryptedFilePath))
        {
            try
            {
                File.Delete(context.EncryptedFilePath);
                _logger.LogDebug("Deleted encrypted temp file: {EncryptedFilePath}", context.EncryptedFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete encrypted temp file: {EncryptedFilePath}", context.EncryptedFilePath);
            }
        }

        _logger.LogInformation("Cleanup completed for FileId: {FileId}", context.FileId);

        return Task.FromResult(context);
    }
}
