using System.Text.Json;

namespace FileIngestionService.Application.DTOs;

public record FileProcessedEvent
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public string? Hash { get; init; }
    public string? MinioObjectKey { get; init; }
    public DateTime ProcessedAt { get; init; }

    public static FileProcessedEvent? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FileProcessedEvent>(json, new JsonSerializerOptions
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
