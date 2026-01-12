using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;

namespace FileProcessingPipeline.Application.Pipeline;

public class FileProcessingPipeline : IPipeline
{
    private readonly IEnumerable<IFilter> _filters;
    private readonly ILogger<FileProcessingPipeline> _logger;

    public FileProcessingPipeline(IEnumerable<IFilter> filters, ILogger<FileProcessingPipeline> logger)
    {
        _filters = filters.OrderBy(f => f.Order);
        _logger = logger;
    }

    public async Task<PipelineContext> ExecuteAsync(PipelineContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting pipeline execution for FileId: {FileId}, CorrelationId: {CorrelationId}",
            context.FileId, context.CorrelationId);

        foreach (var filter in _filters)
        {
            if (context.HasErrors)
            {
                _logger.LogWarning("Pipeline aborted due to errors. Skipping filter: {FilterName}", filter.Name);
                break;
            }

            try
            {
                _logger.LogInformation("Executing filter: {FilterName} (Order: {Order})", filter.Name, filter.Order);
                context = await filter.ProcessAsync(context, cancellationToken);
                context.ProcessedFilters.Add(filter.Name);
                _logger.LogInformation("Filter {FilterName} completed successfully", filter.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Filter {FilterName} failed with error: {Error}", filter.Name, ex.Message);
                context.Errors.Add($"Filter '{filter.Name}' failed: {ex.Message}");
            }
        }

        context.CompletedAt = DateTime.UtcNow;
        
        _logger.LogInformation(
            "Pipeline execution completed for FileId: {FileId}. Processed filters: {FilterCount}, Errors: {ErrorCount}",
            context.FileId, context.ProcessedFilters.Count, context.Errors.Count);

        return context;
    }
}
