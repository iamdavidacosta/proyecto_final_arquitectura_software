using System.Text;
using System.Text.Json;
using FileIngestionService.Application.DTOs;
using FileIngestionService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace FileIngestionService.Infrastructure.Messaging;

public class RabbitMQEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQEventPublisher(IConfiguration configuration, ILogger<RabbitMQEventPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
            Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: _configuration["RabbitMQ:Exchange"] ?? "file-exchange",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false
        );
    }

    public Task PublishFileUploadedAsync(FileUploadedEvent @event, CancellationToken cancellationToken = default)
    {
        var exchange = _configuration["RabbitMQ:Exchange"] ?? "file-exchange";
        var routingKey = _configuration["RabbitMQ:RoutingKey"] ?? "file.uploaded";

        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // persistent
        properties.ContentType = "application/json";
        properties.CorrelationId = @event.CorrelationId.ToString();
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = nameof(FileUploadedEvent);
        properties.AppId = "file-ingestion-service";
        properties.Headers = new Dictionary<string, object>
        {
            ["x-correlation-id"] = @event.CorrelationId.ToString(),
            ["x-source-service"] = "file-ingestion-service"
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation("Published FileUploadedEvent for file {FileId} with correlation {CorrelationId}", 
            @event.FileId, @event.CorrelationId);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
