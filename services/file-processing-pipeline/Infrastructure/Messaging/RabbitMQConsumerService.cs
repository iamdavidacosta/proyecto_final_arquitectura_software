using System.Text;
using FileProcessingPipeline.Application.DTOs;
using FileProcessingPipeline.Domain.Entities;
using FileProcessingPipeline.Domain.Interfaces;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileProcessingPipeline.Infrastructure.Messaging;

public class RabbitMQConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RabbitMQConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeRabbitMQAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    private async Task InitializeRabbitMQAsync(CancellationToken cancellationToken)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception, "Failed to connect to RabbitMQ. Retry {RetryCount} in {TimeSpan}s",
                        retryCount, timeSpan.TotalSeconds);
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "rabbitmq",
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:UserName"] ?? "admin",
                Password = _configuration["RabbitMQ:Password"] ?? "admin123",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var exchange = _configuration["RabbitMQ:Exchange"] ?? "file-exchange";
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "documents";
            var routingKey = _configuration["RabbitMQ:RoutingKey"] ?? "file.uploaded";

            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queueName, exchange, routingKey);

            // Prefetch count for fair dispatch
            _channel.BasicQos(0, 1, false);

            _logger.LogInformation("Connected to RabbitMQ. Queue: {QueueName}, Exchange: {Exchange}", queueName, exchange);
            
            await Task.CompletedTask;
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("RabbitMQ channel not initialized");
            return;
        }

        var queueName = _configuration["RabbitMQ:QueueName"] ?? "documents";

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Received message from queue: {QueueName}", queueName);

            try
            {
                await ProcessMessageAsync(message, stoppingToken);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message. Requeuing...");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("Started consuming from queue: {QueueName}", queueName);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(string message, CancellationToken cancellationToken)
    {
        var fileEvent = FileUploadedEvent.FromJson(message);
        if (fileEvent == null)
        {
            _logger.LogError("Failed to deserialize message: {Message}", message);
            return;
        }

        _logger.LogInformation("Processing file: {FileId}, FileName: {FileName}, CorrelationId: {CorrelationId}",
            fileEvent.FileId, fileEvent.FileName, fileEvent.CorrelationId);

        using var scope = _serviceProvider.CreateScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IPipeline>();

        var context = new PipelineContext
        {
            FileId = fileEvent.FileId,
            UserId = fileEvent.UserId,
            OriginalFileName = fileEvent.FileName,
            ContentType = fileEvent.ContentType,
            OriginalFileSize = fileEvent.FileSize,
            TempFilePath = fileEvent.StoragePath,
            Description = fileEvent.Description,
            CorrelationId = fileEvent.CorrelationId.ToString()
        };

        var result = await pipeline.ExecuteAsync(context, cancellationToken);

        if (result.HasErrors)
        {
            _logger.LogWarning("Pipeline completed with errors for FileId: {FileId}. Errors: {Errors}",
                fileEvent.FileId, string.Join("; ", result.Errors));
        }
        else
        {
            _logger.LogInformation("Pipeline completed successfully for FileId: {FileId}", fileEvent.FileId);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
