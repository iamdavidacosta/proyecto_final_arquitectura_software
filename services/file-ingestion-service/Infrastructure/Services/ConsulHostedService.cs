using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileIngestionService.Infrastructure.Services;

public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConsulHostedService> _logger;
    private string? _serviceId;

    public ConsulHostedService(
        IConsulClient consulClient,
        IConfiguration configuration,
        ILogger<ConsulHostedService> logger)
    {
        _consulClient = consulClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _serviceId = $"file-ingestion-service-{Guid.NewGuid():N}";

        var registration = new AgentServiceRegistration
        {
            ID = _serviceId,
            Name = "file-ingestion-service",
            Address = "file-ingestion-service",
            Port = 8080,
            Tags = new[] { "files", "upload", "api", "dotnet", "signalr" },
            Check = new AgentServiceCheck
            {
                HTTP = "http://file-ingestion-service:8080/health",
                Interval = TimeSpan.FromSeconds(30),
                Timeout = TimeSpan.FromSeconds(10),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        try
        {
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
            _logger.LogInformation("Registered service {ServiceId} with Consul", _serviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register service with Consul");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_serviceId != null)
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(_serviceId, cancellationToken);
                _logger.LogInformation("Deregistered service {ServiceId} from Consul", _serviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deregister service from Consul");
            }
        }
    }
}
