using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileProcessingPipeline.Tests.Filters;

public class CleanupFilterTests
{
    private readonly Mock<ILogger<CleanupFilter>> _loggerMock;
    private readonly CleanupFilter _cleanupFilter;

    public CleanupFilterTests()
    {
        _loggerMock = new Mock<ILogger<CleanupFilter>>();
        _cleanupFilter = new CleanupFilter(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_ShouldDeleteLocalFile()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFilePath, new byte[] { 1, 2, 3 });

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            LocalFilePath = tempFilePath,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        File.Exists(tempFilePath).Should().BeTrue();

        // Act
        var result = await _cleanupFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        File.Exists(tempFilePath).Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_ShouldDeleteEncryptedFile()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        var encryptedFilePath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFilePath, new byte[] { 1, 2, 3 });
        await File.WriteAllBytesAsync(encryptedFilePath, new byte[] { 4, 5, 6 });

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            LocalFilePath = tempFilePath,
            EncryptedFilePath = encryptedFilePath,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        File.Exists(encryptedFilePath).Should().BeTrue();

        // Act
        var result = await _cleanupFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        File.Exists(encryptedFilePath).Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentFiles_ShouldReturnTrue()
    {
        // Arrange
        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            LocalFilePath = "/nonexistent/path/file.txt",
            EncryptedFilePath = "/nonexistent/path/encrypted.txt",
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        // Act
        var result = await _cleanupFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeTrue(); // Should succeed even if files don't exist
    }

    [Fact]
    public async Task ProcessAsync_ShouldMarkAsCompleted()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFilePath, new byte[] { 1, 2, 3 });

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test.txt",
            LocalFilePath = tempFilePath,
            ContentType = "text/plain",
            UserId = Guid.NewGuid(),
            IsCompleted = false
        };

        // Act
        var result = await _cleanupFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        context.IsCompleted.Should().BeTrue();
    }
}
