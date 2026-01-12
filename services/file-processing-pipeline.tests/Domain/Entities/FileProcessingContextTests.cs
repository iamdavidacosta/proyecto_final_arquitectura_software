using FileProcessingPipeline.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace FileProcessingPipeline.Tests.Domain.Entities;

public class FileProcessingContextTests
{
    [Fact]
    public void FileProcessingContext_ShouldInitializeWithDefaults()
    {
        // Act
        var context = new FileProcessingContext();

        // Assert
        context.FileId.Should().Be(Guid.Empty);
        context.OriginalFileName.Should().BeNull();
        context.LocalFilePath.Should().BeNull();
        context.ContentType.Should().BeNull();
        context.UserId.Should().Be(Guid.Empty);
        context.IsEncrypted.Should().BeFalse();
        context.IsCompleted.Should().BeFalse();
        context.Metadata.Should().NotBeNull();
        context.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void FileProcessingContext_ShouldStoreAllProperties()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var originalFileName = "document.pdf";
        var localFilePath = "/tmp/uploads/doc.pdf";
        var encryptedFilePath = "/tmp/encrypted/doc.enc";
        var contentType = "application/pdf";
        var fileHash = "abc123def456";
        var fileSize = 1024L;
        var minioPath = "bucket/files/doc.pdf";
        var encryptionKeyId = "key-001";
        var processedAt = DateTime.UtcNow;

        // Act
        var context = new FileProcessingContext
        {
            FileId = fileId,
            OriginalFileName = originalFileName,
            LocalFilePath = localFilePath,
            EncryptedFilePath = encryptedFilePath,
            ContentType = contentType,
            UserId = userId,
            FileHash = fileHash,
            FileSize = fileSize,
            MinioPath = minioPath,
            EncryptionKeyId = encryptionKeyId,
            IsEncrypted = true,
            IsCompleted = true,
            ProcessedAt = processedAt
        };

        // Assert
        context.FileId.Should().Be(fileId);
        context.OriginalFileName.Should().Be(originalFileName);
        context.LocalFilePath.Should().Be(localFilePath);
        context.EncryptedFilePath.Should().Be(encryptedFilePath);
        context.ContentType.Should().Be(contentType);
        context.UserId.Should().Be(userId);
        context.FileHash.Should().Be(fileHash);
        context.FileSize.Should().Be(fileSize);
        context.MinioPath.Should().Be(minioPath);
        context.EncryptionKeyId.Should().Be(encryptionKeyId);
        context.IsEncrypted.Should().BeTrue();
        context.IsCompleted.Should().BeTrue();
        context.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void FileProcessingContext_Metadata_ShouldBeModifiable()
    {
        // Arrange
        var context = new FileProcessingContext();

        // Act
        context.Metadata["key1"] = "value1";
        context.Metadata["key2"] = "value2";

        // Assert
        context.Metadata.Should().HaveCount(2);
        context.Metadata["key1"].Should().Be("value1");
        context.Metadata["key2"].Should().Be("value2");
    }

    [Fact]
    public void FileProcessingContext_ErrorMessage_ShouldBeSettable()
    {
        // Arrange
        var context = new FileProcessingContext();
        var errorMessage = "Processing failed: File corrupted";

        // Act
        context.ErrorMessage = errorMessage;

        // Assert
        context.ErrorMessage.Should().Be(errorMessage);
    }
}
