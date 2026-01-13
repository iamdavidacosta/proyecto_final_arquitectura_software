using FileProcessingPipeline.Application.Filters;
using FileProcessingPipeline.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FileProcessingPipeline.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register all filters in order:
        // 1. HashFilter (Order=1) - Compute SHA-256 hash
        // 2. MetadataFilter (Order=2) - Extract file metadata
        // 3. EncryptFilter (Order=3) - Encrypt with RSA+AES
        // 4. DecryptValidationFilter (Order=4) - Validate decryption works
        // 5. MinioUploadFilter (Order=5) - Upload original and encrypted to MinIO
        // 6. MongoStorageFilter (Order=6) - Store metadata in MongoDB
        // 100. CleanupFilter (Order=100) - Clean up temp files
        
        services.AddSingleton<IFilter, HashFilter>();
        services.AddSingleton<IFilter, MetadataFilter>();
        services.AddSingleton<IFilter, EncryptFilter>();
        services.AddSingleton<IFilter, DecryptValidationFilter>();
        services.AddSingleton<IFilter, MinioUploadFilter>();
        services.AddSingleton<IFilter, MongoStorageFilter>();
        services.AddSingleton<IFilter, CleanupFilter>();

        // Register pipeline
        services.AddSingleton<IPipeline, Pipeline.FileProcessingPipeline>();

        return services;
    }
}
