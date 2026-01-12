// Dependency Injection for SOAP Service Infrastructure - Updated
using Consul;
using Minio;
using MongoDB.Driver;
using SoapService.Domain.Interfaces;
using SoapService.Infrastructure.Persistence;
using SoapService.Infrastructure.Services;

namespace SoapService.Infrastructure;

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
        services.AddSingleton<IMinioService, MinioService>();

        // Hosted Services
        services.AddHostedService<ConsulHostedService>();

        return services;
    }
}
