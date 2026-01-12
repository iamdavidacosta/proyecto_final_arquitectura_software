using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileProcessingPipeline.Tests.Filters;

public class MetadataFilterTests
{
    private readonly Mock<ILogger<MetadataFilter>> _loggerMock;
    private readonly MetadataFilter _metadataFilter;

    public MetadataFilterTests()
    {
        _loggerMock = new Mock<ILogger<MetadataFilter>>();
        _metadataFilter = new MetadataFilter(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithValidFile_ShouldExtractMetadata()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        var testContent = new byte[1024]; // 1KB file
        new Random().NextBytes(testContent);
        await File.WriteAllBytesAsync(tempFilePath, testContent);

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
            var result = await _metadataFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.FileSize.Should().Be(1024);
            context.Metadata.Should().NotBeNull();
            context.Metadata.Should().ContainKey("extension");
            context.Metadata["extension"].Should().Be(".tmp");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task ProcessAsync_ShouldExtractCorrectExtension()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var tempFilePath = Path.Combine(tempDir, $"{Guid.NewGuid()}.pdf");
        await File.WriteAllBytesAsync(tempFilePath, new byte[] { 1, 2, 3, 4 });

        var context = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "document.pdf",
            LocalFilePath = tempFilePath,
            ContentType = "application/pdf",
            UserId = Guid.NewGuid()
        };

        try
        {
            // Act
            var result = await _metadataFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.Metadata["extension"].Should().Be(".pdf");
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task ProcessAsync_ShouldSetProcessedAtTimestamp()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFilePath, new byte[] { 1, 2, 3 });

        var beforeProcess = DateTime.UtcNow;

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
            var result = await _metadataFilter.ProcessAsync(context, CancellationToken.None);
            var afterProcess = DateTime.UtcNow;

            // Assert
            result.Should().BeTrue();
            context.ProcessedAt.Should().BeOnOrAfter(beforeProcess);
            context.ProcessedAt.Should().BeOnOrBefore(afterProcess);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
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
        var result = await _metadataFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAsync_EmptyFile_ShouldSetFileSizeToZero()
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
            var result = await _metadataFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.FileSize.Should().Be(0);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}
