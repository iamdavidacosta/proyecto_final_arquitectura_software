using Consul;
using FileIngestionService.Application.Interfaces;
using FileIngestionService.Domain.Interfaces;
using FileIngestionService.Infrastructure.Messaging;
using FileIngestionService.Infrastructure.Persistence;
using FileIngestionService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FileIngestionService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("MySQL");
        services.AddDbContext<FileDbContext>(options =>
            options.UseMySql(connectionString ?? throw new InvalidOperationException("MySQL connection string not found"),
                ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // RabbitMQ
        services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
        services.AddHostedService<FileProcessedConsumer>();

        // Consul
        var consulHost = configuration["Consul:Host"] ?? "localhost";
        var consulPort = configuration.GetValue<int>("Consul:Port", 8500);

        services.AddSingleton<IConsulClient>(_ => new ConsulClient(config =>
        {
            config.Address = new Uri($"http://{consulHost}:{consulPort}");
        }));

        services.AddHostedService<ConsulHostedService>();

        // OpenTelemetry
        var otelEndpoint = configuration["OpenTelemetry:Endpoint"];
        if (!string.IsNullOrEmpty(otelEndpoint))
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("file-ingestion-service"))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint)))
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint)));
        }

        return services;
    }
}
