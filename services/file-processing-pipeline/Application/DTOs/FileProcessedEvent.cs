using System.Text.Json;

namespace FileProcessingPipeline.Application.DTOs;

public record FileProcessedEvent
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // "Completed" or "Failed"
    public string? ErrorMessage { get; init; }
    public string? Hash { get; init; }
    public string? MinioObjectKey { get; init; }
    public DateTime ProcessedAt { get; init; }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}
