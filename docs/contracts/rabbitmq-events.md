# RabbitMQ Events Contract

## Exchange Configuration

### Exchange: `file-exchange`
- **Type**: `direct`
- **Durable**: `true`
- **Auto-delete**: `false`

## Queues

### Queue: `documents`
- **Durable**: `true`
- **Exclusive**: `false`
- **Auto-delete**: `false`
- **Arguments**:
  - `x-message-ttl`: 86400000 (24 hours)
  - `x-dead-letter-exchange`: `file-exchange-dlx`
  - `x-dead-letter-routing-key`: `documents.dead`

### Queue: `documents-dead` (Dead Letter Queue)
- **Durable**: `true`
- **Exclusive**: `false`
- **Auto-delete**: `false`

## Bindings

| Exchange | Routing Key | Queue |
|----------|-------------|-------|
| `file-exchange` | `file.uploaded` | `documents` |
| `file-exchange-dlx` | `documents.dead` | `documents-dead` |

## Event: FileUploaded

### Routing Key
```
file.uploaded
```

### JSON Schema
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "FileUploaded",
  "type": "object",
  "required": [
    "fileId",
    "correlationId",
    "userId",
    "fileName",
    "contentType",
    "fileSize",
    "uploadedAt",
    "status"
  ],
  "properties": {
    "fileId": {
      "type": "string",
      "format": "uuid",
      "description": "Unique identifier for the file"
    },
    "correlationId": {
      "type": "string",
      "format": "uuid",
      "description": "Correlation ID for distributed tracing"
    },
    "userId": {
      "type": "string",
      "format": "uuid",
      "description": "ID of the user who uploaded the file"
    },
    "userEmail": {
      "type": "string",
      "format": "email",
      "description": "Email of the user who uploaded the file"
    },
    "fileName": {
      "type": "string",
      "minLength": 1,
      "maxLength": 255,
      "description": "Original file name"
    },
    "contentType": {
      "type": "string",
      "description": "MIME type of the file"
    },
    "fileSize": {
      "type": "integer",
      "minimum": 1,
      "description": "File size in bytes"
    },
    "uploadedAt": {
      "type": "string",
      "format": "date-time",
      "description": "ISO 8601 timestamp of upload"
    },
    "status": {
      "type": "string",
      "enum": ["Received"],
      "description": "Initial status of the file"
    },
    "storagePath": {
      "type": "string",
      "description": "Temporary storage path in the service"
    }
  }
}
```

### Example Payload
```json
{
  "fileId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "123e4567-e89b-12d3-a456-426614174000",
  "userId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "userEmail": "john.doe@example.com",
  "fileName": "document.pdf",
  "contentType": "application/pdf",
  "fileSize": 1048576,
  "uploadedAt": "2024-01-15T10:30:00Z",
  "status": "Received",
  "storagePath": "/tmp/uploads/550e8400-e29b-41d4-a716-446655440000"
}
```

## Event: FileProcessed (Internal)

### Routing Key
```
file.processed
```

### JSON Schema
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "FileProcessed",
  "type": "object",
  "required": [
    "fileId",
    "correlationId",
    "status",
    "processedAt"
  ],
  "properties": {
    "fileId": {
      "type": "string",
      "format": "uuid"
    },
    "correlationId": {
      "type": "string",
      "format": "uuid"
    },
    "status": {
      "type": "string",
      "enum": [
        "MetadataStored",
        "Hashed",
        "Encrypted",
        "DecryptedValidated",
        "UploadedToMinIO",
        "Failed"
      ]
    },
    "processedAt": {
      "type": "string",
      "format": "date-time"
    },
    "sha256Hash": {
      "type": "string",
      "pattern": "^[a-fA-F0-9]{64}$",
      "description": "SHA-256 hash of the file (if status >= Hashed)"
    },
    "originalMinioPath": {
      "type": "string",
      "description": "MinIO path for original file"
    },
    "encryptedMinioPath": {
      "type": "string",
      "description": "MinIO path for encrypted file"
    },
    "errorMessage": {
      "type": "string",
      "description": "Error message if status is Failed"
    },
    "pipelineStages": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "stageName": { "type": "string" },
          "status": { "type": "string", "enum": ["Completed", "Failed", "Skipped"] },
          "startedAt": { "type": "string", "format": "date-time" },
          "completedAt": { "type": "string", "format": "date-time" },
          "error": { "type": "string" }
        }
      }
    }
  }
}
```

### Example Payload (Success)
```json
{
  "fileId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "UploadedToMinIO",
  "processedAt": "2024-01-15T10:31:00Z",
  "sha256Hash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "originalMinioPath": "original-files/550e8400-e29b-41d4-a716-446655440000/document.pdf",
  "encryptedMinioPath": "encrypted-files/550e8400-e29b-41d4-a716-446655440000/document.pdf.enc",
  "pipelineStages": [
    {
      "stageName": "MetadataExtraction",
      "status": "Completed",
      "startedAt": "2024-01-15T10:30:05Z",
      "completedAt": "2024-01-15T10:30:10Z"
    },
    {
      "stageName": "HashGeneration",
      "status": "Completed",
      "startedAt": "2024-01-15T10:30:10Z",
      "completedAt": "2024-01-15T10:30:20Z"
    },
    {
      "stageName": "Encryption",
      "status": "Completed",
      "startedAt": "2024-01-15T10:30:20Z",
      "completedAt": "2024-01-15T10:30:40Z"
    },
    {
      "stageName": "DecryptionValidation",
      "status": "Completed",
      "startedAt": "2024-01-15T10:30:40Z",
      "completedAt": "2024-01-15T10:30:50Z"
    },
    {
      "stageName": "MinIOUpload",
      "status": "Completed",
      "startedAt": "2024-01-15T10:30:50Z",
      "completedAt": "2024-01-15T10:31:00Z"
    }
  ]
}
```

### Example Payload (Failed)
```json
{
  "fileId": "550e8400-e29b-41d4-a716-446655440000",
  "correlationId": "123e4567-e89b-12d3-a456-426614174000",
  "status": "Failed",
  "processedAt": "2024-01-15T10:30:25Z",
  "errorMessage": "Failed to connect to MongoDB: Connection timeout",
  "pipelineStages": [
    {
      "stageName": "MetadataExtraction",
      "status": "Failed",
      "startedAt": "2024-01-15T10:30:05Z",
      "completedAt": "2024-01-15T10:30:25Z",
      "error": "Connection timeout after 20 seconds"
    }
  ]
}
```

## Message Properties

### Standard AMQP Properties
```json
{
  "contentType": "application/json",
  "contentEncoding": "utf-8",
  "deliveryMode": 2,
  "priority": 0,
  "correlationId": "123e4567-e89b-12d3-a456-426614174000",
  "messageId": "msg-550e8400-e29b-41d4-a716-446655440001",
  "timestamp": 1704067200,
  "type": "FileUploaded",
  "appId": "file-ingestion-service"
}
```

### Custom Headers
```json
{
  "x-correlation-id": "123e4567-e89b-12d3-a456-426614174000",
  "x-source-service": "file-ingestion-service",
  "x-retry-count": 0,
  "x-original-timestamp": "2024-01-15T10:30:00Z"
}
```

## Consumer Configuration

### Prefetch Count
```
prefetchCount: 10
```

### Acknowledgement
- **Mode**: Manual acknowledgement
- **Ack on**: Successful processing
- **Nack on**: Processing failure (requeue = false, goes to DLQ)

### Retry Policy
- **Max Retries**: 3
- **Backoff**: Exponential (1s, 2s, 4s)
- **After Max Retries**: Send to Dead Letter Queue

## Producer Configuration

### Confirmation Mode
```
publisherConfirms: true
```

### Mandatory Flag
```
mandatory: true
```

## .NET Implementation Example

### Publisher
```csharp
public class FileUploadedEventPublisher : IEventPublisher<FileUploadedEvent>
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    
    public async Task PublishAsync(FileUploadedEvent @event, CancellationToken ct)
    {
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
        
        var body = JsonSerializer.SerializeToUtf8Bytes(@event);
        
        _channel.BasicPublish(
            exchange: "file-exchange",
            routingKey: "file.uploaded",
            basicProperties: properties,
            body: body
        );
    }
}
```

### Consumer
```csharp
public class FileUploadedEventConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var @event = JsonSerializer.Deserialize<FileUploadedEvent>(body);
                
                await _pipeline.ProcessAsync(@event, stoppingToken);
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message");
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };
        
        _channel.BasicConsume(queue: "documents", autoAck: false, consumer: consumer);
        
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
```

## Monitoring

### Metrics to Track
- `rabbitmq_messages_published_total`
- `rabbitmq_messages_consumed_total`
- `rabbitmq_messages_failed_total`
- `rabbitmq_message_processing_duration_seconds`
- `rabbitmq_dlq_messages_total`

### Alerts
- DLQ message count > 10
- Consumer lag > 100 messages
- Connection failures
