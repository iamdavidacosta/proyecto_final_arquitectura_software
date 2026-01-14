using System.Text;
using FileProcessingPipeline.Application.DTOs;
using FileProcessingPipeline.Domain.Interfaces;
using RabbitMQ.Client;

namespace FileProcessingPipeline.Infrastructure.Messaging;

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
            HostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
            Password = _configuration["RabbitMQ:Password"] ?? "admin123"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the exchange for processed events
        _channel.ExchangeDeclare(
            exchange: _configuration["RabbitMQ:Exchange"] ?? "file-exchange",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );
    }

    public Task PublishFileProcessedAsync(FileProcessedEvent @event, CancellationToken cancellationToken = default)
    {
        var exchange = _configuration["RabbitMQ:Exchange"] ?? "file-exchange";
        var routingKey = "file.processed";

        var properties = _channel.CreateBasicProperties();
        properties.DeliveryMode = 2; // persistent
        properties.ContentType = "application/json";
        properties.CorrelationId = @event.CorrelationId;
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = nameof(FileProcessedEvent);
        properties.AppId = "file-processing-pipeline";
        properties.Headers = new Dictionary<string, object>
        {
            ["x-correlation-id"] = @event.CorrelationId,
            ["x-source-service"] = "file-processing-pipeline"
        };

        var body = Encoding.UTF8.GetBytes(@event.ToJson());

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );

        _logger.LogInformation("Published FileProcessedEvent for file {FileId} with status {Status}", 
            @event.FileId, @event.Status);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
