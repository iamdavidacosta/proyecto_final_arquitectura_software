using FileProcessingPipeline.Application.Filters;
using FileProcessingPipeline.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FileProcessingPipeline.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all filters
        services.AddSingleton<IFilter, HashFilter>();
        services.AddSingleton<IFilter, MetadataFilter>();
        services.AddSingleton<IFilter, EncryptFilter>();
        services.AddSingleton<IFilter, MinioUploadFilter>();
        services.AddSingleton<IFilter, MongoStorageFilter>();
        services.AddSingleton<IFilter, CleanupFilter>();

        // Register pipeline
        services.AddSingleton<IPipeline, Pipeline.FileProcessingPipeline>();

        return services;
    }
}
