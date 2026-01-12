#!/bin/bash

# ===========================================
# FileShare Platform - Quick Start Script
# ===========================================

set -e

echo "🚀 Starting FileShare Platform..."
echo ""

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if .env file exists
if [ ! -f .env ]; then
    echo "📝 Creating .env file from template..."
    cp .env.example .env
    echo "✅ .env file created. You may want to customize it."
fi

# Generate encryption keys if they don't exist
if [ ! -f infra/keys/private.pem ]; then
    echo "🔐 Generating encryption keys..."
    openssl genrsa -out infra/keys/private.pem 2048
    openssl rsa -in infra/keys/private.pem -pubout -out infra/keys/public.pem
    chmod 600 infra/keys/private.pem
    echo "✅ Encryption keys generated."
fi

# Set MongoDB keyfile permissions
chmod 400 infra/mongodb/keyfile 2>/dev/null || true

# Build and start all services
echo ""
echo "🐳 Building and starting Docker containers..."
echo "This may take several minutes on first run..."
echo ""

docker compose up --build -d

echo ""
echo "⏳ Waiting for services to be healthy..."
sleep 30

# Check service health
echo ""
echo "📊 Service Status:"
docker compose ps

echo ""
echo "✅ FileShare Platform is starting!"
echo ""
echo "🌐 Access Points:"
echo "   - Frontend:        http://localhost:3000"
echo "   - API Gateway:     http://localhost:5000"
echo "   - Spring Viewer:   http://localhost:8080"
echo "   - Auth Service:    http://localhost:5001"
echo "   - File Ingestion:  http://localhost:5002"
echo "   - SOAP Service:    http://localhost:5003/FileService.svc?wsdl"
echo ""
echo "📊 Monitoring:"
echo "   - Grafana:         http://localhost:3001 (admin/Grafana@Admin!2024)"
echo "   - Prometheus:      http://localhost:9090"
echo "   - Consul:          http://localhost:8500"
echo "   - RabbitMQ:        http://localhost:15672 (rabbitmq_user/R@bbitMQ!2024)"
echo "   - MinIO:           http://localhost:9001 (minioadmin/Mini0@Admin!2024)"
echo "   - Mongo Express:   http://localhost:8081"
echo ""
echo "📝 Logs: docker compose logs -f [service-name]"
echo "🛑 Stop: docker compose down"
echo ""
