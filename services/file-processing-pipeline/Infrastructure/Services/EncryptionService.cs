using System.Security.Cryptography;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Infrastructure.Services;

/// <summary>
/// Encryption service using hybrid encryption:
/// - RSA (asymmetric) to encrypt the AES key
/// - AES-256 (symmetric) to encrypt the file content
/// This provides the security of asymmetric encryption with the performance of symmetric encryption.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EncryptionService> _logger;
    private readonly string _publicKeyPath;
    private readonly string _privateKeyPath;

    public EncryptionService(IConfiguration configuration, ILogger<EncryptionService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _publicKeyPath = configuration["Encryption:PublicKeyPath"] ?? "/app/keys/public.pem";
        _privateKeyPath = configuration["Encryption:PrivateKeyPath"] ?? "/app/keys/private.pem";
    }

    public async Task<string> EncryptFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Encrypting file using hybrid RSA+AES: {InputPath} -> {OutputPath}", inputPath, outputPath);

        // Generate AES key and IV for symmetric encryption
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        // Encrypt AES key with RSA public key (asymmetric encryption)
        byte[] encryptedAesKey;
        using (var rsa = RSA.Create())
        {
            var publicKeyPem = await LoadKeyAsync(_publicKeyPath, cancellationToken);
            rsa.ImportFromPem(publicKeyPem);
            
            // Encrypt AES key and IV together
            var keyAndIv = new byte[aes.Key.Length + aes.IV.Length];
            Buffer.BlockCopy(aes.Key, 0, keyAndIv, 0, aes.Key.Length);
            Buffer.BlockCopy(aes.IV, 0, keyAndIv, aes.Key.Length, aes.IV.Length);
            
            encryptedAesKey = rsa.Encrypt(keyAndIv, RSAEncryptionPadding.OaepSHA256);
            _logger.LogDebug("AES key encrypted with RSA. Encrypted key length: {Length} bytes", encryptedAesKey.Length);
        }

        await using var outputStream = File.Create(outputPath);
        
        // Write encrypted key length (4 bytes) and encrypted AES key
        var keyLengthBytes = BitConverter.GetBytes(encryptedAesKey.Length);
        await outputStream.WriteAsync(keyLengthBytes, cancellationToken);
        await outputStream.WriteAsync(encryptedAesKey, cancellationToken);

        // Encrypt file content with AES
        await using var inputStream = File.OpenRead(inputPath);
        await using var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);
        
        await inputStream.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        
        _logger.LogInformation("File encrypted successfully with RSA+AES hybrid encryption: {OutputPath}", outputPath);
        return outputPath;
    }

    public async Task<string> DecryptFileAsync(string inputPath, string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Decrypting file using hybrid RSA+AES: {InputPath} -> {OutputPath}", inputPath, outputPath);

        await using var inputStream = File.OpenRead(inputPath);
        
        // Read encrypted key length
        var keyLengthBytes = new byte[4];
        await inputStream.ReadExactlyAsync(keyLengthBytes, cancellationToken);
        var encryptedKeyLength = BitConverter.ToInt32(keyLengthBytes);
        
        // Read encrypted AES key
        var encryptedAesKey = new byte[encryptedKeyLength];
        await inputStream.ReadExactlyAsync(encryptedAesKey, cancellationToken);

        // Decrypt AES key with RSA private key (asymmetric decryption)
        byte[] keyAndIv;
        using (var rsa = RSA.Create())
        {
            var privateKeyPem = await LoadKeyAsync(_privateKeyPath, cancellationToken);
            rsa.ImportFromPem(privateKeyPem);
            
            keyAndIv = rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.OaepSHA256);
            _logger.LogDebug("AES key decrypted with RSA private key");
        }

        // Extract AES key and IV
        var aesKey = new byte[32]; // 256-bit key
        var aesIv = new byte[16];  // 128-bit IV
        Buffer.BlockCopy(keyAndIv, 0, aesKey, 0, aesKey.Length);
        Buffer.BlockCopy(keyAndIv, aesKey.Length, aesIv, 0, aesIv.Length);

        using var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIv;

        // Decrypt file content with AES
        await using var outputStream = File.Create(outputPath);
        await using var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
        
        await cryptoStream.CopyToAsync(outputStream, cancellationToken);
        
        _logger.LogInformation("File decrypted successfully: {OutputPath}", outputPath);
        return outputPath;
    }

    private async Task<string> LoadKeyAsync(string keyPath, CancellationToken cancellationToken)
    {
        if (!File.Exists(keyPath))
        {
            _logger.LogWarning("Key file not found at {KeyPath}, generating new RSA key pair", keyPath);
            await GenerateKeyPairAsync(cancellationToken);
        }
        
        return await File.ReadAllTextAsync(keyPath, cancellationToken);
    }

    private async Task GenerateKeyPairAsync(CancellationToken cancellationToken)
    {
        using var rsa = RSA.Create(2048);
        
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        var publicKey = rsa.ExportRSAPublicKeyPem();
        
        var keysDirectory = Path.GetDirectoryName(_privateKeyPath);
        if (!string.IsNullOrEmpty(keysDirectory) && !Directory.Exists(keysDirectory))
        {
            Directory.CreateDirectory(keysDirectory);
        }
        
        await File.WriteAllTextAsync(_privateKeyPath, privateKey, cancellationToken);
        await File.WriteAllTextAsync(_publicKeyPath, publicKey, cancellationToken);
        
        _logger.LogInformation("Generated new RSA key pair at {PublicKeyPath} and {PrivateKeyPath}", 
            _publicKeyPath, _privateKeyPath);
    }
}
