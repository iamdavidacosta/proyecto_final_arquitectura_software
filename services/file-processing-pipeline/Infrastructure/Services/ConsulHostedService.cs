using Consul;

namespace FileProcessingPipeline.Infrastructure.Services;

public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConsulHostedService> _logger;
    private string? _registrationId;
    private Timer? _healthCheckTimer;

    public ConsulHostedService(IConsulClient consulClient, IConfiguration configuration, ILogger<ConsulHostedService> logger)
    {
        _consulClient = consulClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var serviceName = _configuration["Consul:ServiceName"] ?? "file-processing-pipeline";
        var serviceHost = _configuration["Consul:ServiceHost"] ?? "file-processing-pipeline";
        var servicePort = int.Parse(_configuration["Consul:ServicePort"] ?? "80");

        _registrationId = $"{serviceName}-{Guid.NewGuid():N}";

        var registration = new AgentServiceRegistration
        {
            ID = _registrationId,
            Name = serviceName,
            Address = serviceHost,
            Port = servicePort,
            Tags = new[] { "file-processing", "worker", "pipeline" },
            Check = new AgentServiceCheck
            {
                TTL = TimeSpan.FromSeconds(30),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        try
        {
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
            _logger.LogInformation("Service registered with Consul: {ServiceName} ({RegistrationId})", serviceName, _registrationId);
            
            // Start periodic health check update for TTL
            _healthCheckTimer = new Timer(async _ => await UpdateHealthCheck(), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register service with Consul");
        }
    }

    private async Task UpdateHealthCheck()
    {
        try
        {
            await _consulClient.Agent.PassTTL($"service:{_registrationId}", "Service is running");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update TTL check");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _healthCheckTimer?.Dispose();
        
        if (_registrationId != null)
        {
            try
            {
                await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
                _logger.LogInformation("Service deregistered from Consul: {RegistrationId}", _registrationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deregister service from Consul");
            }
        }
    }
}
