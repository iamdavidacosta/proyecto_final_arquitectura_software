using FileProcessingPipeline.Domain.Entities;

namespace FileProcessingPipeline.Domain.Interfaces;

public interface IPipeline
{
    Task<PipelineContext> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken = default);
}
