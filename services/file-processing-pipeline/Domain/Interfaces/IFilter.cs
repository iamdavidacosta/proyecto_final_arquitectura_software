using FileProcessingPipeline.Domain.Entities;

namespace FileProcessingPipeline.Domain.Interfaces;

public interface IFilter
{
    string Name { get; }
    int Order { get; }
    Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default);
}
