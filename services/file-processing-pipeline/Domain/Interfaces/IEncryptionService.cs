namespace FileProcessingPipeline.Domain.Interfaces;

public interface IEncryptionService
{
    Task<string> EncryptFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);
    Task<string> DecryptFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default);
}
