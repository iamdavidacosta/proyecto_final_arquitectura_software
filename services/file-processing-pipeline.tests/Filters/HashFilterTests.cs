using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Filters;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FileProcessingPipeline.Tests.Filters;

public class HashFilterTests
{
    private readonly Mock<ILogger<HashFilter>> _loggerMock;
    private readonly HashFilter _hashFilter;

    public HashFilterTests()
    {
        _loggerMock = new Mock<ILogger<HashFilter>>();
        _hashFilter = new HashFilter(_loggerMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_WithValidFile_ShouldComputeHash()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        var testContent = "This is test content for hashing"u8.ToArray();
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
            var result = await _hashFilter.ProcessAsync(context, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            context.FileHash.Should().NotBeNullOrEmpty();
            context.FileHash.Should().HaveLength(64); // SHA256 hash length in hex
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task ProcessAsync_SameContent_ShouldProduceSameHash()
    {
        // Arrange
        var tempFilePath1 = Path.GetTempFileName();
        var tempFilePath2 = Path.GetTempFileName();
        var testContent = "Identical content"u8.ToArray();
        await File.WriteAllBytesAsync(tempFilePath1, testContent);
        await File.WriteAllBytesAsync(tempFilePath2, testContent);

        var context1 = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test1.txt",
            LocalFilePath = tempFilePath1,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        var context2 = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test2.txt",
            LocalFilePath = tempFilePath2,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        try
        {
            // Act
            await _hashFilter.ProcessAsync(context1, CancellationToken.None);
            await _hashFilter.ProcessAsync(context2, CancellationToken.None);

            // Assert
            context1.FileHash.Should().Be(context2.FileHash);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath1)) File.Delete(tempFilePath1);
            if (File.Exists(tempFilePath2)) File.Delete(tempFilePath2);
        }
    }

    [Fact]
    public async Task ProcessAsync_DifferentContent_ShouldProduceDifferentHash()
    {
        // Arrange
        var tempFilePath1 = Path.GetTempFileName();
        var tempFilePath2 = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFilePath1, "Content A"u8.ToArray());
        await File.WriteAllBytesAsync(tempFilePath2, "Content B"u8.ToArray());

        var context1 = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test1.txt",
            LocalFilePath = tempFilePath1,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        var context2 = new FileProcessingContext
        {
            FileId = Guid.NewGuid(),
            OriginalFileName = "test2.txt",
            LocalFilePath = tempFilePath2,
            ContentType = "text/plain",
            UserId = Guid.NewGuid()
        };

        try
        {
            // Act
            await _hashFilter.ProcessAsync(context1, CancellationToken.None);
            await _hashFilter.ProcessAsync(context2, CancellationToken.None);

            // Assert
            context1.FileHash.Should().NotBe(context2.FileHash);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath1)) File.Delete(tempFilePath1);
            if (File.Exists(tempFilePath2)) File.Delete(tempFilePath2);
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
        var result = await _hashFilter.ProcessAsync(context, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }
}
