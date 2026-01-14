using System.Text;
using FileIngestionService.API.Hubs;
using FileIngestionService.Application.DTOs;
using FileIngestionService.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FileIngestionService.Infrastructure.Messaging;

public class FileProcessedConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileProcessedConsumer> _logger;
    private readonly IHubContext<FileUploadHub> _hubContext;
    private IConnection? _connection;
    private IModel? _channel;

    public FileProcessedConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<FileProcessedConsumer> logger,
        IHubContext<FileUploadHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
        _hubContext = hubContext;
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
                HostName = _configuration["RabbitMQ:Host"] ?? "rabbitmq",
                Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
                UserName = _configuration["RabbitMQ:Username"] ?? "admin",
                Password = _configuration["RabbitMQ:Password"] ?? "admin123",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            var exchange = _configuration["RabbitMQ:Exchange"] ?? "file-exchange";
            var queueName = "file-processed-updates";
            var routingKey = "file.processed";

            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queueName, exchange, routingKey);
            _channel.BasicQos(0, 1, false);

            _logger.LogInformation("FileProcessedConsumer connected to RabbitMQ. Queue: {QueueName}", queueName);
            
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

        var queueName = "file-processed-updates";

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("Received FileProcessedEvent from queue");

            try
            {
                await ProcessMessageAsync(message, stoppingToken);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing FileProcessedEvent. Requeuing...");
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
        var processedEvent = FileProcessedEvent.FromJson(message);
        if (processedEvent == null)
        {
            _logger.LogError("Failed to deserialize FileProcessedEvent: {Message}", message);
            return;
        }

        _logger.LogInformation("Processing FileProcessedEvent for FileId: {FileId}, Status: {Status}",
            processedEvent.FileId, processedEvent.Status);

        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // Update file status in MySQL
        var file = await unitOfWork.Files.GetByIdAsync(processedEvent.FileId, cancellationToken);
        if (file != null)
        {
            file.UpdateStatus(processedEvent.Status);
            if (!string.IsNullOrEmpty(processedEvent.Hash))
            {
                file.SetHash(processedEvent.Hash);
            }
            if (!string.IsNullOrEmpty(processedEvent.MinioObjectKey))
            {
                file.SetMinioPaths(string.Empty, processedEvent.MinioObjectKey);
            }
            
            await unitOfWork.Files.UpdateAsync(file, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated file {FileId} status to {Status} in MySQL", 
                processedEvent.FileId, processedEvent.Status);

            // Notify ALL connected clients via SignalR (so all users see the update)
            await _hubContext.Clients.All
                .SendAsync("FileProcessed", new
                {
                    fileId = processedEvent.FileId.ToString(),
                    status = processedEvent.Status,
                    processedAt = processedEvent.ProcessedAt,
                    userId = processedEvent.UserId.ToString()
                }, cancellationToken);

            _logger.LogInformation("Sent SignalR notification to ALL clients for file {FileId}",
                processedEvent.FileId);
        }
        else
        {
            _logger.LogWarning("File {FileId} not found in database", processedEvent.FileId);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
