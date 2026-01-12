@echo off
REM ===========================================
REM FileShare Platform - Quick Start Script (Windows)
REM ===========================================

echo.
echo 🚀 Starting FileShare Platform...
echo.

REM Check if Docker is running
docker info >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not running. Please start Docker and try again.
    exit /b 1
)

REM Check if .env file exists
if not exist .env (
    echo 📝 Creating .env file from template...
    copy .env.example .env
    echo ✅ .env file created. You may want to customize it.
)

REM Generate encryption keys if they don't exist
if not exist infra\keys\private.pem (
    echo 🔐 Generating encryption keys...
    openssl genrsa -out infra\keys\private.pem 2048
    openssl rsa -in infra\keys\private.pem -pubout -out infra\keys\public.pem
    echo ✅ Encryption keys generated.
)

REM Build and start all services
echo.
echo 🐳 Building and starting Docker containers...
echo This may take several minutes on first run...
echo.

docker compose up --build -d

echo.
echo ⏳ Waiting for services to be healthy...
timeout /t 30 /nobreak >nul

REM Check service health
echo.
echo 📊 Service Status:
docker compose ps

echo.
echo ✅ FileShare Platform is starting!
echo.
echo 🌐 Access Points:
echo    - Frontend:        http://localhost:3000
echo    - API Gateway:     http://localhost:5000
echo    - Spring Viewer:   http://localhost:8080
echo    - Auth Service:    http://localhost:5001
echo    - File Ingestion:  http://localhost:5002
echo    - SOAP Service:    http://localhost:5003/FileService.svc?wsdl
echo.
echo 📊 Monitoring:
echo    - Grafana:         http://localhost:3001 (admin/Grafana@Admin!2024)
echo    - Prometheus:      http://localhost:9090
echo    - Consul:          http://localhost:8500
echo    - RabbitMQ:        http://localhost:15672 (rabbitmq_user/R@bbitMQ!2024)
echo    - MinIO:           http://localhost:9001 (minioadmin/Mini0@Admin!2024)
echo    - Mongo Express:   http://localhost:8081
echo.
echo 📝 Logs: docker compose logs -f [service-name]
echo 🛑 Stop: docker compose down
echo.
pause
