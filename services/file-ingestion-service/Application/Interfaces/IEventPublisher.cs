using FileIngestionService.Application.DTOs;

namespace FileIngestionService.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishFileUploadedAsync(FileUploadedEvent @event, CancellationToken cancellationToken = default);
}
