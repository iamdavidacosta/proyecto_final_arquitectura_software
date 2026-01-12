# 🏛️ Arquitectura del Sistema - File Share Platform

## Visión General

Este documento describe la arquitectura de la plataforma de compartición de archivos, implementada como un sistema de microservicios siguiendo principios de Clean Architecture, SOLID, y patrones de diseño empresariales.

## Principios Arquitectónicos

### Clean Architecture
Cada microservicio sigue la estructura de capas concéntricas:

```
┌──────────────────────────────────────┐
│           Presentation/API           │  ← Controllers, Middleware
├──────────────────────────────────────┤
│            Application               │  ← Use Cases, DTOs, Validators
├──────────────────────────────────────┤
│              Domain                  │  ← Entities, Value Objects, Interfaces
├──────────────────────────────────────┤
│           Infrastructure             │  ← Repositories, External Services
└──────────────────────────────────────┘
```

**Regla de Dependencia**: Las dependencias solo fluyen hacia adentro. Las capas internas no conocen las externas.

### SOLID Principles
- **S**ingle Responsibility: Cada clase tiene una única razón para cambiar
- **O**pen/Closed: Extensible sin modificación
- **L**iskov Substitution: Tipos derivados sustituibles
- **I**nterface Segregation: Interfaces específicas y cohesivas
- **D**ependency Inversion: Dependencia de abstracciones

## Microservicios

### 1. Auth Service (Puerto 5001)

**Responsabilidad**: Autenticación y autorización de usuarios.

```
Tecnologías: .NET 8, MySQL, JWT
Endpoints:
  POST /api/auth/register  - Registro de usuario
  POST /api/auth/login     - Login y emisión de JWT
  POST /api/auth/refresh   - Renovación de token
  GET  /api/auth/validate  - Validación de token
```

**Flujo de Autenticación**:
```
Usuario → Register/Login → Validación → JWT (HS256) → Response
```

### 2. File Ingestion Service (Puerto 5002)

**Responsabilidad**: Recepción de archivos con comunicación real-time.

```
Tecnologías: .NET 8, SignalR, MySQL, RabbitMQ
Endpoints:
  POST /api/files/upload   - Upload tradicional
  HUB  /hubs/file-upload   - SignalR para progreso real-time
```

**Flujo de Upload**:
```
┌─────────┐    SignalR     ┌─────────────────┐
│ Frontend │◄──────────────►│ File Ingestion  │
└─────────┘   (progress)    │    Service      │
                            └────────┬────────┘
                                     │
            ┌────────────────────────┼────────────────────────┐
            ▼                        ▼                        ▼
    ┌──────────────┐        ┌──────────────┐        ┌──────────────┐
    │    MySQL     │        │   RabbitMQ   │        │   Response   │
    │(file record) │        │ (documents)  │        │  (FileId)    │
    └──────────────┘        └──────────────┘        └──────────────┘
```

### 3. File Processing Pipeline (Worker Service)

**Responsabilidad**: Procesamiento asíncrono con patrón Pipes & Filters.

```
Tecnologías: .NET 8 Worker, RabbitMQ, MongoDB, MinIO
Patrón: Pipes & Filters (chain of responsibility)
```

**Pipeline de Filtros**:
```
┌─────────────┐   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐   ┌─────────────┐
│  Metadata   │──►│    Hash     │──►│  Encrypt    │──►│  Decrypt    │──►│   Upload    │
│   Filter    │   │   Filter    │   │   Filter    │   │   Filter    │   │   Filter    │
│  (MongoDB)  │   │  (SHA-256)  │   │   (RSA)     │   │(Validation) │   │   (MinIO)   │
└─────────────┘   └─────────────┘   └─────────────┘   └─────────────┘   └─────────────┘
       │                 │                 │                 │                 │
       ▼                 ▼                 ▼                 ▼                 ▼
   MongoDB           MySQL            Memory            Memory             MinIO
  (metadata)         (hash)        (encrypted)       (validated)         (storage)
```

**Estados del Pipeline**:
1. `Received` - Archivo recibido del queue
2. `MetadataStored` - Metadata guardada en MongoDB
3. `Hashed` - Hash SHA-256 calculado y guardado
4. `Encrypted` - Archivo encriptado con RSA
5. `DecryptedValidated` - Validación de integridad
6. `UploadedToMinIO` - Archivos subidos a MinIO
7. `Failed` - Error en cualquier etapa

### 4. SOAP Service (Puerto 5003)

**Responsabilidad**: Exponer información consolidada via SOAP/WSDL.

```
Tecnologías: .NET 8, SoapCore, MySQL, MongoDB
Endpoint: /FileService.svc
WSDL: /FileService.svc?wsdl
```

**Operaciones SOAP**:
- `GetAllFiles` - Lista todos los archivos
- `GetFileById` - Detalle de archivo por ID
- `GetPipelineStatus` - Estado del pipeline por FileId

### 5. REST Service (Puerto 5004)

**Responsabilidad**: API REST para acceso a archivos con agregación.

```
Tecnologías: .NET 8, OpenAPI/Swagger
Endpoints:
  GET  /api/files           - Listar archivos
  GET  /api/files/{id}      - Detalle de archivo
  GET  /api/files/{id}/download/original   - Descargar original
  GET  /api/files/{id}/download/encrypted  - Descargar encriptado
```

**Arquitectura de Agregación**:
```
┌──────────────┐
│ REST Service │
└──────┬───────┘
       │
       ├──────► SOAP Service (información procesada)
       │
       └──────► MinIO (URLs de descarga)
```

### 6. API Gateway (Puerto 5000)

**Responsabilidad**: Punto único de entrada, routing, autenticación.

```
Tecnologías: .NET 8, YARP (Yet Another Reverse Proxy)
```

**Funcionalidades**:
- JWT Validation
- Request routing
- Rate limiting
- Load balancing
- Health aggregation
- Consul integration (service discovery)

**Routing Table**:
```yaml
/api/auth/*    → auth-service:5001
/api/files/*   → file-ingestion-service:5002
/api/rest/*    → nginx-lb (→ rest-service:5004 x2)
/soap/*        → soap-service:5003
/hubs/*        → file-ingestion-service:5002 (WebSocket)
```

### 7. Spring Visualizer (Puerto 8080)

**Responsabilidad**: Aplicación web de visualización consumiendo SOAP.

```
Tecnologías: Spring Boot 3, Thymeleaf, Resilience4j
Páginas:
  /              - Dashboard
  /files         - Lista de archivos
  /files/{id}    - Detalle de archivo
```

## Infraestructura

### Base de Datos

#### MySQL Cluster (Writer + 2 Replicas)
```
┌─────────────────┐
│  mysql-writer   │◄─── Writes
│    (primary)    │
└────────┬────────┘
         │ replication
    ┌────┴────┐
    ▼         ▼
┌────────┐ ┌────────┐
│replica1│ │replica2│◄─── Reads
└────────┘ └────────┘
```

#### ProxySQL (Load Balancer)
- Puerto: 6033
- Enruta escrituras al writer
- Distribuye lecturas entre réplicas
- Health monitoring

#### MongoDB Replica Set
```
┌─────────────────┐
│mongodb-primary  │◄─── Writes
│   (primary)     │
└────────┬────────┘
         │ replication
    ┌────┴────┐
    ▼         ▼
┌──────────┐ ┌──────────┐
│secondary1│ │secondary2│◄─── Reads
└──────────┘ └──────────┘
```

### Mensajería

#### RabbitMQ
```
Exchange: file-exchange (direct)
Queue: documents
Binding: file.uploaded → documents
```

### Object Storage

#### MinIO
```
Buckets:
  - original-files    (archivos originales)
  - encrypted-files   (archivos encriptados)
```

### Service Discovery

#### Consul
- Service Registration
- Health Checks
- KV Store (configuración distribuida)
- DNS Interface

### Load Balancing

#### Nginx (REST Service)
```nginx
upstream rest-service {
    server rest-service-1:5004;
    server rest-service-2:5004;
}
```

## Patrones de Resiliencia

### Circuit Breaker (Polly / Resilience4j)
```
Estados:
  Closed  → Normal operation
  Open    → Fail fast (después de N fallos)
  HalfOpen → Testing recovery
```

**Configuración .NET (Polly)**:
```csharp
services.AddHttpClient<ISoapClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

**Configuración Spring (Resilience4j)**:
```yaml
resilience4j:
  circuitbreaker:
    instances:
      soapService:
        failureRateThreshold: 50
        waitDurationInOpenState: 5000
```

### Retry con Exponential Backoff
- Intento 1: inmediato
- Intento 2: 2s delay
- Intento 3: 4s delay
- Intento 4: 8s delay
- Max: 4 intentos

## Observabilidad

### Métricas (Prometheus)
```
Métricas expuestas por servicio:
  - http_requests_total
  - http_request_duration_seconds
  - pipeline_files_processed_total
  - pipeline_processing_duration_seconds
```

### Logging (Serilog)
```json
{
  "Timestamp": "2024-01-15T10:30:00Z",
  "Level": "Information",
  "MessageTemplate": "File {FileId} processed",
  "Properties": {
    "FileId": "uuid",
    "CorrelationId": "uuid",
    "Service": "file-processing-pipeline"
  }
}
```

### Tracing (OpenTelemetry)
- CorrelationId propagado via headers
- Spans por operación
- Exportación a Jaeger/Tempo

### Health Checks
```
GET /health
{
  "status": "Healthy",
  "checks": {
    "mysql": "Healthy",
    "rabbitmq": "Healthy",
    "mongodb": "Healthy"
  }
}
```

## Seguridad

### JWT Token Flow
```
1. Usuario → Login → Auth Service
2. Auth Service → Validate credentials → Generate JWT
3. JWT → Response → Frontend (memoria)
4. Frontend → Request + JWT Header → API Gateway
5. Gateway → Validate JWT → Route to Service
```

### Claims Estándar
```json
{
  "sub": "user-id-uuid",
  "email": "user@example.com",
  "role": "user|admin",
  "iat": 1704067200,
  "exp": 1704070800,
  "iss": "file-share-platform",
  "aud": "file-share-users"
}
```

## Despliegue

### Docker Compose Networks
```yaml
networks:
  frontend-net:    # React ↔ Gateway
  backend-net:     # Services intercommunication
  data-net:        # Services ↔ Databases
  monitoring-net:  # Prometheus ↔ Services
```

### Resource Limits
```yaml
deploy:
  resources:
    limits:
      cpus: '0.5'
      memory: 512M
    reservations:
      cpus: '0.25'
      memory: 256M
```

## Decisiones Arquitectónicas (ADRs)

### ADR-001: YARP vs Ocelot para API Gateway
**Decisión**: YARP
**Razón**: Mayor rendimiento, soporte activo de Microsoft, configuración flexible.

### ADR-002: SoapCore vs CoreWCF
**Decisión**: SoapCore
**Razón**: Más ligero, mejor integración con ASP.NET Core, suficiente para los requerimientos.

### ADR-003: ProxySQL vs HAProxy para MySQL
**Decisión**: ProxySQL
**Razón**: Diseñado específicamente para MySQL, query routing inteligente, connection pooling nativo.

### ADR-004: MongoDB Replica Set Size
**Decisión**: 3 nodos (1 primary + 2 secondary)
**Razón**: Mínimo para fault tolerance, automatic failover.

### ADR-005: Formato de Almacenamiento en MinIO
**Decisión**: Separar buckets original/encrypted
**Razón**: Facilita políticas de acceso diferenciadas y lifecycle management.
