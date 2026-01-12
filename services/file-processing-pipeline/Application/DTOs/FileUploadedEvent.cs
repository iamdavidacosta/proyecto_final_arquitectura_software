using System.Text.Json;

namespace FileProcessingPipeline.Application.DTOs;

public record FileUploadedEvent
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string TempFilePath { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }

    public static FileUploadedEvent? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FileUploadedEvent>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}
