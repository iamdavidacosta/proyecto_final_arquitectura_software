using System.Security.Cryptography;
using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Filters;

/// <summary>
/// Computes SHA256 hash of the file for integrity verification
/// </summary>
public class HashFilter : IFilter
{
    private readonly ILogger<HashFilter> _logger;

    public string Name => "HashFilter";
    public int Order => 2;

    public HashFilter(ILogger<HashFilter> logger)
    {
        _logger = logger;
    }

    public async Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Computing hash for file: {FilePath}", context.TempFilePath);

        if (!File.Exists(context.TempFilePath))
        {
            context.Errors.Add($"File not found: {context.TempFilePath}");
            return context;
        }

        using var sha256 = SHA256.Create();
        await using var stream = File.OpenRead(context.TempFilePath);
        
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        context.Hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        context.HashAlgorithm = "SHA256";

        _logger.LogInformation("Hash computed: {Hash}", context.Hash);

        return context;
    }
}
