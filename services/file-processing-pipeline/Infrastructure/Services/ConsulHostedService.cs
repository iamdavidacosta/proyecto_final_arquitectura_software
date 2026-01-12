using Consul;

namespace FileProcessingPipeline.Infrastructure.Services;

public class ConsulHostedService : IHostedService
{
    private readonly IConsulClient _consulClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConsulHostedService> _logger;
    private string? _registrationId;

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
                TCP = $"{serviceHost}:{servicePort}",
                Interval = TimeSpan.FromSeconds(10),
                Timeout = TimeSpan.FromSeconds(5),
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            }
        };

        try
        {
            await _consulClient.Agent.ServiceRegister(registration, cancellationToken);
            _logger.LogInformation("Service registered with Consul: {ServiceName} ({RegistrationId})", serviceName, _registrationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register service with Consul");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
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
