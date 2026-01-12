using Consul;
using FileProcessingPipeline.Domain.Interfaces;
using FileProcessingPipeline.Infrastructure.Messaging;
using FileProcessingPipeline.Infrastructure.Persistence;
using FileProcessingPipeline.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using MongoDB.Driver;

namespace FileProcessingPipeline.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB
        services.AddSingleton<IMongoClient>(sp =>
        {
            var connectionString = configuration["MongoDB:ConnectionString"] 
                ?? "mongodb://admin:admin123@mongodb-primary:27017/?replicaSet=rs0";
            return new MongoClient(connectionString);
        });

        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var databaseName = configuration["MongoDB:DatabaseName"] ?? "fileshare_metadata";
            return client.GetDatabase(databaseName);
        });

        // MinIO
        services.AddSingleton<IMinioClient>(sp =>
        {
            var endpoint = configuration["MinIO:Endpoint"] ?? "minio:9000";
            var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
            var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin123";
            var useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");

            return new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSSL)
                .Build();
        });

        // Consul - hardcoded to prevent configuration issues
        services.AddSingleton<IConsulClient>(sp =>
        {
            const string consulHost = "http://consul:8500";
            return new ConsulClient(config =>
            {
                config.Address = new Uri(consulHost);
            });
        });

        // Repositories
        services.AddSingleton<IFileMetadataRepository, FileMetadataRepository>();

        // Services
        services.AddSingleton<IMinioService, MinioService>();
        services.AddSingleton<IEncryptionService, EncryptionService>();

        // Hosted Services
        services.AddHostedService<ConsulHostedService>();
        services.AddHostedService<RabbitMQConsumerService>();

        return services;
    }
}
