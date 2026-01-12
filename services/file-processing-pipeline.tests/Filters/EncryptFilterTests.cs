using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Filters;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileProcessingPipeline.Tests.Filters;

public class EncryptFilterTests
{
    private readonly Mock<ILogger<EncryptFilter>> _loggerMock;
    private readonly IConfiguration _configuration;
    private readonly EncryptFilter _encryptFilter;
    private readonly string _testKeyPath;

    public EncryptFilterTests()
    {
        _loggerMock = new Mock<ILogger<EncryptFilter>>();
        
        // Create a temp key file for testing
        _testKeyPath = Path.Combine(Path.GetTempPath(), "test_encryption_key.pem");
        
        // Generate a simple test key (in real scenario, use proper RSA key)
        if (!File.Exists(_testKeyPath))
        {
            // Create a simple placeholder for testing
            File.WriteAllText(_testKeyPath, "-----BEGIN PUBLIC KEY-----\nTest key content\n-----END PUBLIC KEY-----");
        }

        var configurationData = new Dictionary<string, string?>
        {
            { "Encryption:PublicKeyPath", _testKeyPath },
            { "Encryption:Algorithm", "AES256" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        _encryptFilter = new EncryptFilter(_configuration, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithValidFile_ShouldEncryptFile()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        var originalContent = "This is secret content that needs encryption"u8.ToArray();
        await File.WriteAllBytesAsync(tempFilePath, originalContent);

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "secret.txt",
            LocalFilePath = tempFilePath,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        try
        {
            // Act
            var result = await _encryptFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.EncryptedFilePath.Should().NotBeNullOrEmpty();
            context.IsEncrypted.Should().BeTrue();
            
            if (File.Exists(context.EncryptedFilePath))
            {
                var encryptedContent = await File.ReadAllBytesAsync(context.EncryptedFilePath);
                encryptedContent.Should().NotBeEquivalentTo(originalContent);
            }
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
            if (!string.IsNullOrEmpty(context.EncryptedFilePath) && File.Exists(context.EncryptedFilePath))
                File.Delete(context.EncryptedFilePath);
        }
    }

    [Fact]
    public async Task ProcessAsync_ShouldSetEncryptionKeyId()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFilePath, "Content"u8.ToArray());

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            LocalFilePath = tempFilePath,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        try
        {
            // Act
            var result = await _encryptFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.EncryptionKeyId.Should().NotBeNullOrEmpty();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
            if (!string.IsNullOrEmpty(context.EncryptedFilePath) && File.Exists(context.EncryptedFilePath))
                File.Delete(context.EncryptedFilePath);
        }
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentFile_ShouldReturnFalse()
    {
        // Arrange
        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "nonexistent.txt",
            LocalFilePath = "/nonexistent/path/file.txt",
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _encryptFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        context.IsEncrypted.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_EmptyFile_ShouldStillEncrypt()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        // File is already empty

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "empty.txt",
            LocalFilePath = tempFilePath,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        try
        {
            // Act
            var result = await _encryptFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.IsEncrypted.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
            if (!string.IsNullOrEmpty(context.EncryptedFilePath) && File.Exists(context.EncryptedFilePath))
                File.Delete(context.EncryptedFilePath);
        }
    }
}
