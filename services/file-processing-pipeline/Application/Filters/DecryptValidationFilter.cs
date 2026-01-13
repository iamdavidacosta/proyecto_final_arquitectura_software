using System.Security.Cryptography;
using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Decrypts the file to validate that encryption was successful.
/// Compares the hash of the decrypted file with the original file hash.
/// </summary>
public class DecryptValidationFilter : IFilter
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<DecryptValidationFilter> _logger;

    public string Name => "DecryptValidationFilter";
    public int Order => 4; // After encryption (3), before MinIO upload (5)

    public DecryptValidationFilter(IEncryptionService encryptionService, ILogger<DecryptValidationFilter> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        if (!context.IsEncrypted || string.IsNullOrEmpty(context.EncryptedFilePath))
        {
            _logger.LogWarning("File is not encrypted, skipping decryption validation");
            return context;
        }

        _logger.LogInformation("Validating decryption for file: {FileName}", context.OriginalFileName);

        var decryptedPath = context.TempFilePath + ".decrypted";

        try
        {
            // Decrypt the encrypted file
            await _encryptionService.DecryptFileAsync(context.EncryptedFilePath, decryptedPath, cancellationToken);

            // Compute hash of decrypted file
            using var sha256 = SHA256.Create();
            await using var decryptedStream = File.OpenRead(decryptedPath);
            var decryptedHashBytes = await sha256.ComputeHashAsync(decryptedStream, cancellationToken);
            var decryptedHash = Convert.ToHexString(decryptedHashBytes).ToLowerInvariant();

            // Compare with original hash
            if (decryptedHash == context.Hash)
            {
                context.IsDecryptionValidated = true;
                context.ExtractedMetadata["decryptionValidated"] = true;
                context.ExtractedMetadata["decryptedHash"] = decryptedHash;
                _logger.LogInformation("Decryption validation successful. Hash matches: {Hash}", context.Hash);
            }
            else
            {
                context.IsDecryptionValidated = false;
                context.Errors.Add($"Decryption validation failed: Hash mismatch. Original: {context.Hash}, Decrypted: {decryptedHash}");
                _logger.LogError("Decryption validation failed. Original hash: {OriginalHash}, Decrypted hash: {DecryptedHash}",
                    context.Hash, decryptedHash);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate decryption for file: {FileName}", context.OriginalFileName);
            context.Errors.Add($"Decryption validation failed: {ex.Message}");
            context.IsDecryptionValidated = false;
        }
        finally
        {
            // Clean up decrypted temp file
            if (File.Exists(decryptedPath))
            {
                try
                {
                    File.Delete(decryptedPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete decrypted temp file: {DecryptedPath}", decryptedPath);
                }
            }
        }

        return context;
    }
}
