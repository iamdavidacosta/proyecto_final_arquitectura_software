using AuthService.Application.Interfaces;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Services;
using Consul;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("MySQL");
        services.AddDbContext<AuthDbContext>(options =>
            options.UseMySQL(connectionString ?? throw new InvalidOperationException("MySQL connection string not found")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

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
                .ConfigureResource(resource => resource.AddService("auth-service"))
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
