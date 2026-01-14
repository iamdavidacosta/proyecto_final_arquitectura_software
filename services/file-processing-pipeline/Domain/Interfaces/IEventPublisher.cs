using FileProcessingPipeline.Application.DTOs;

namespace FileProcessingPipeline.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishFileProcessedAsync(FileProcessedEvent @event, CancellationToken cancellationToken = default);
}
