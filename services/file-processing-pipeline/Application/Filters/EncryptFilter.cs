using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Encrypts the file using AES-256 before storage
/// </summary>
public class EncryptFilter : IFilter
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<EncryptFilter> _logger;

    public string Name => "EncryptFilter";
    public int Order => 3;

    public EncryptFilter(IEncryptionService encryptionService, ILogger<EncryptFilter> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Encrypting file: {FileName}", context.OriginalFileName);

        var encryptedPath = context.TempFilePath + ".encrypted";

        try
        {
            await _encryptionService.EncryptFileAsync(context.TempFilePath, encryptedPath, cancellationToken);
            
            context.EncryptedFilePath = encryptedPath;
            context.IsEncrypted = true;
            
            // Update size after encryption
            var encryptedInfo = new FileInfo(encryptedPath);
            context.ExtractedMetadata["encryptedSize"] = encryptedInfo.Length;

            _logger.LogInformation("File encrypted successfully. Encrypted path: {EncryptedPath}", encryptedPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt file: {FileName}", context.OriginalFileName);
            context.Errors.Add($"Encryption failed: {ex.Message}");
        }

        return context;
    }
}
