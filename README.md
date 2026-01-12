# 📁 File Share Platform - Microservices Architecture

## Proyecto Final - Arquitectura de Software

Plataforma de compartición de archivos basada en microservicios con Clean Architecture, implementando patrones como API Gateway, Circuit Breaker, Pipes & Filters, y Service Discovery.

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND (React)                                │
│                          http://localhost:3000                               │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         API GATEWAY (YARP + JWT)                            │
│                          http://localhost:5000                               │
└─────────────────────────────────────────────────────────────────────────────┘
        │                    │                    │                    │
        ▼                    ▼                    ▼                    ▼
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ AUTH-SERVICE │    │FILE-INGESTION│    │  REST-SERVICE │    │ SOAP-SERVICE │
│   (.NET 8)   │    │   (.NET 8)   │    │   (.NET 8)   │    │   (.NET 8)   │
│  Port: 5001  │    │  Port: 5002  │    │  Port: 5004  │    │  Port: 5003  │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
        │                    │                    │                    │
        │                    │ SignalR            │                    │
        │                    │ WebSocket          │                    │
        │                    ▼                    │                    │
        │           ┌──────────────┐              │                    │
        │           │   RabbitMQ   │──────────────┼────────────────────┤
        │           │  (documents) │              │                    │
        │           └──────────────┘              │                    │
        │                    │                    │                    │
        │                    ▼                    │                    │
        │    ┌───────────────────────────────┐   │                    │
        │    │  FILE-PROCESSING-PIPELINE     │   │                    │
        │    │   (.NET Worker Service)       │   │                    │
        │    │   Pipes & Filters Pattern     │   │                    │
        │    │   ┌─────┐ ┌─────┐ ┌─────┐    │   │                    │
        │    │   │Meta │→│Hash │→│Crypt│    │   │                    │
        │    │   └─────┘ └─────┘ └─────┘    │   │                    │
        │    └───────────────────────────────┘   │                    │
        │                    │                    │                    │
        ▼                    ▼                    ▼                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DATA LAYER                                      │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐           │
│  │ MySQL   │  │ProxySQL │  │ MongoDB │  │  MinIO  │  │ Consul  │           │
│  │Cluster  │  │ (L.B.)  │  │ Replica │  │(Storage)│  │(Service │           │
│  │ W+2R    │  │         │  │   Set   │  │         │  │Discovery│           │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘  └─────────┘           │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
┌─────────────────────────────────────────────────────────────────────────────┐
│                           OBSERVABILITY                                      │
│       ┌────────────┐    ┌────────────┐    ┌────────────┐                    │
│       │ Prometheus │    │   Grafana  │    │OpenTelemetry│                   │
│       │            │    │            │    │ Collector   │                   │
│       └────────────┘    └────────────┘    └────────────┘                    │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SPRING VISUALIZER (Spring Boot 3)                         │
│                 Consume SOAP Service - Thymeleaf UI                          │
│                          http://localhost:8080                               │
└─────────────────────────────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerrequisitos

- Docker Desktop 4.x+
- Docker Compose v2.x+
- Mínimo 8GB RAM disponible para Docker
- Puertos disponibles: 3000, 3001, 5000-5004, 8080, 9090, 15672, 9001, 8500

### Ejecución

```bash
# 1. Clonar y navegar al directorio
cd proyecto_final_arquitectura_software

# 2. Copiar archivo de configuración
cp .env.example .env

# 3. Levantar toda la infraestructura
docker-compose up --build -d

# 4. Esperar ~2-3 minutos para inicialización completa

# 5. Verificar salud de servicios
docker-compose ps
```

### URLs de Acceso

| Servicio | URL | Credenciales |
|----------|-----|--------------|
| Frontend React | http://localhost:3000 | - |
| API Gateway | http://localhost:5000 | JWT Required |
| Spring Visualizer | http://localhost:8080 | - |
| RabbitMQ Management | http://localhost:15672 | rabbitmq_user / rabbitmq_pass123 |
| MinIO Console | http://localhost:9001 | minio_admin / minio_pass123 |
| Consul UI | http://localhost:8500 | - |
| Grafana | http://localhost:3001 | admin / admin123 |
| Prometheus | http://localhost:9090 | - |
| Mongo Express | http://localhost:8081 | - |

## 📋 Flujo End-to-End

1. **Registro de Usuario**: El usuario se registra via `POST /api/auth/register`
2. **Login**: El usuario obtiene JWT via `POST /api/auth/login`
3. **Upload de Archivo**: Via SignalR con progreso real-time
4. **Procesamiento Pipeline**:
   - Extracción de metadata → MongoDB
   - Generación de hash SHA-256 → MySQL
   - Encriptación RSA → Validación
   - Upload a MinIO (original + encriptado)
5. **Consulta REST**: Listado y detalle de archivos con metadata agregada
6. **Visualización SOAP**: Spring Boot consume servicio SOAP para reportes

## 🧪 Testing

```bash
# Unit tests - Servicios .NET
cd services/auth-service
dotnet test

# Unit tests - Pipeline
cd services/file-processing-pipeline
dotnet test

# Integration tests
docker-compose -f docker-compose.test.yml up --build
```

## 📚 Documentación

- [Arquitectura Detallada](./docs/architecture.md)
- [Contrato JWT](./docs/contracts/jwt.md)
- [Eventos RabbitMQ](./docs/contracts/rabbitmq-events.md)
- [OpenAPI Specification](./docs/contracts/openapi.yaml)
- [SOAP WSDL](./docs/contracts/soap-wsdl.md)

## 🔧 Patrones Implementados

| Patrón | Implementación |
|--------|----------------|
| API Gateway | YARP (.NET) |
| Circuit Breaker | Polly (.NET), Resilience4j (Spring) |
| Service Discovery | Consul |
| Load Balancer | Nginx (REST Service) |
| Pipes & Filters | Pipeline Worker Service |
| Event-Driven | RabbitMQ |
| CQRS (parcial) | Read replicas MySQL |

## 🛡️ Seguridad

- JWT con RS256 para autenticación
- Secrets via environment variables
- No credenciales hardcodeadas
- HTTPS ready (configurar certs para producción)
- Rate limiting en API Gateway

## 📊 Observabilidad

- **Tracing**: OpenTelemetry → Jaeger/Tempo
- **Metrics**: Prometheus + Grafana dashboards
- **Logging**: Serilog estructurado
- **Health Checks**: `/health` endpoint por servicio

## 🏛️ Clean Architecture por Servicio

```
service/
├── src/
│   ├── Domain/           # Entidades, Value Objects, Interfaces
│   ├── Application/      # Use Cases, DTOs, Validators
│   ├── Infrastructure/   # Repositories, External Services
│   └── API/              # Controllers, Middleware, DI
├── tests/
│   ├── Unit/
│   └── Integration/
└── Dockerfile
```

## 📝 Licencia

MIT License - Proyecto Académico