using System.Security.Cryptography;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> EncryptFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Encrypting file: {InputPath} -> {OutputPath}", inputPath, outputPath);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        // In production, you'd encrypt the AES key with RSA public key
        // For simplicity, we'll store key and IV at the beginning of the file
        await using var outputStream = File.Create(outputPath);
        
        // Write key and IV (in production, encrypt these with RSA)
        await outputStream.WriteAsync(aes.Key, cancellationToken);
        await outputStream.WriteAsync(aes.IV, cancellationToken);

        await using var inputStream = File.OpenRead(inputPath);
        await using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        
        await inputStream.CopyToAsync(cryptoStream, cancellationToken);
        
        _logger.LogInformation("File encrypted successfully: {OutputPath}", outputPath);
        return outputPath;
    }

    public async Task<string> DecryptFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Decrypting file: {InputPath} -> {OutputPath}", inputPath, outputPath);

        await using var inputStream = File.OpenRead(inputPath);
        
        // Read key and IV from the beginning of the file
        var key = new byte[32]; // 256-bit key
        var iv = new byte[16];  // 128-bit IV
        
        await inputStream.ReadExactlyAsync(key, cancellationToken);
        await inputStream.ReadExactlyAsync(iv, cancellationToken);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        await using var outputStream = File.Create(outputPath);
        await using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        
        await cryptoStream.CopyToAsync(outputStream, cancellationToken);
        
        _logger.LogInformation("File decrypted successfully: {OutputPath}", outputPath);
        return outputPath;
    }
}
