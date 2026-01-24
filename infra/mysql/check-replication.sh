#!/bin/bash
# ===========================================
# MySQL Replication Health Check Script
# Verifica el estado de la replicación
# ===========================================

# Variables
ROOT_PASSWORD="${MYSQL_ROOT_PASSWORD:-RootP@ssw0rd!2024}"
MASTER_HOST="mysql-writer"
REPLICA1_HOST="mysql-replica1"
REPLICA2_HOST="mysql-replica2"

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo ""
echo "============================================="
echo "  MySQL Replication Health Check"
echo "============================================="
echo ""

# Verificar Master
echo "📦 MASTER ($MASTER_HOST)"
echo "-------------------------------------------"
MASTER_STATUS=$(mysql -h "$MASTER_HOST" -uroot -p"$ROOT_PASSWORD" -e "SHOW MASTER STATUS\G" 2>/dev/null)
if [ $? -eq 0 ]; then
    MASTER_LOG_FILE=$(echo "$MASTER_STATUS" | grep "File:" | awk '{print $2}')
    MASTER_LOG_POS=$(echo "$MASTER_STATUS" | grep "Position:" | awk '{print $2}')
    echo -e "   Estado: ${GREEN}✅ ONLINE${NC}"
    echo "   Binary Log: $MASTER_LOG_FILE"
    echo "   Position: $MASTER_LOG_POS"
else
    echo -e "   Estado: ${RED}❌ OFFLINE${NC}"
fi
echo ""

# Función para verificar réplica
check_replica() {
    local host=$1
    local name=$2
    
    echo "📋 $name ($host)"
    echo "-------------------------------------------"
    
    SLAVE_STATUS=$(mysql -h "$host" -uroot -p"$ROOT_PASSWORD" -e "SHOW SLAVE STATUS\G" 2>/dev/null)
    
    if [ $? -ne 0 ]; then
        echo -e "   Estado: ${RED}❌ NO CONECTADO${NC}"
        return 1
    fi
    
    SLAVE_IO=$(echo "$SLAVE_STATUS" | grep "Slave_IO_Running:" | awk '{print $2}')
    SLAVE_SQL=$(echo "$SLAVE_STATUS" | grep "Slave_SQL_Running:" | awk '{print $2}')
    SECONDS_BEHIND=$(echo "$SLAVE_STATUS" | grep "Seconds_Behind_Master:" | awk '{print $2}')
    MASTER_HOST_CONN=$(echo "$SLAVE_STATUS" | grep "Master_Host:" | awk '{print $2}')
    LAST_IO_ERROR=$(echo "$SLAVE_STATUS" | grep "Last_IO_Error:" | cut -d':' -f2- | xargs)
    LAST_SQL_ERROR=$(echo "$SLAVE_STATUS" | grep "Last_SQL_Error:" | cut -d':' -f2- | xargs)
    RELAY_LOG_FILE=$(echo "$SLAVE_STATUS" | grep "Relay_Log_File:" | awk '{print $2}')
    
    # Estado general
    if [ "$SLAVE_IO" = "Yes" ] && [ "$SLAVE_SQL" = "Yes" ]; then
        echo -e "   Estado: ${GREEN}✅ SINCRONIZADO${NC}"
    elif [ "$SLAVE_IO" = "Connecting" ]; then
        echo -e "   Estado: ${YELLOW}⏳ CONECTANDO...${NC}"
    else
        echo -e "   Estado: ${RED}❌ ERROR${NC}"
    fi
    
    # Detalles
    echo "   Master Host: $MASTER_HOST_CONN"
    
    if [ "$SLAVE_IO" = "Yes" ]; then
        echo -e "   Slave_IO_Running: ${GREEN}Yes${NC}"
    else
        echo -e "   Slave_IO_Running: ${RED}$SLAVE_IO${NC}"
    fi
    
    if [ "$SLAVE_SQL" = "Yes" ]; then
        echo -e "   Slave_SQL_Running: ${GREEN}Yes${NC}"
    else
        echo -e "   Slave_SQL_Running: ${RED}$SLAVE_SQL${NC}"
    fi
    
    # Lag
    if [ "$SECONDS_BEHIND" = "0" ]; then
        echo -e "   Seconds_Behind_Master: ${GREEN}$SECONDS_BEHIND${NC}"
    elif [ "$SECONDS_BEHIND" = "NULL" ]; then
        echo -e "   Seconds_Behind_Master: ${RED}NULL (no conectado)${NC}"
    else
        echo -e "   Seconds_Behind_Master: ${YELLOW}$SECONDS_BEHIND${NC}"
    fi
    
    echo "   Relay Log: $RELAY_LOG_FILE"
    
    # Errores
    if [ -n "$LAST_IO_ERROR" ] && [ "$LAST_IO_ERROR" != "" ]; then
        echo -e "   ${RED}Last_IO_Error: $LAST_IO_ERROR${NC}"
    fi
    
    if [ -n "$LAST_SQL_ERROR" ] && [ "$LAST_SQL_ERROR" != "" ]; then
        echo -e "   ${RED}Last_SQL_Error: $LAST_SQL_ERROR${NC}"
    fi
    
    echo ""
}

# Verificar réplicas
check_replica "$REPLICA1_HOST" "REPLICA 1"
check_replica "$REPLICA2_HOST" "REPLICA 2"

# Test de replicación
echo "🧪 TEST DE REPLICACIÓN"
echo "-------------------------------------------"

# Crear tabla de prueba y insertar datos
TEST_VALUE="test_$(date +%s)"
mysql -h "$MASTER_HOST" -uroot -p"$ROOT_PASSWORD" -e "
    CREATE DATABASE IF NOT EXISTS test_replication;
    USE test_replication;
    CREATE TABLE IF NOT EXISTS ping_test (id INT AUTO_INCREMENT PRIMARY KEY, value VARCHAR(100), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP);
    INSERT INTO ping_test (value) VALUES ('$TEST_VALUE');
" 2>/dev/null

sleep 1

# Verificar en réplicas
for replica in "$REPLICA1_HOST" "$REPLICA2_HOST"; do
    RESULT=$(mysql -h "$replica" -uroot -p"$ROOT_PASSWORD" -N -e "SELECT value FROM test_replication.ping_test ORDER BY id DESC LIMIT 1;" 2>/dev/null)
    if [ "$RESULT" = "$TEST_VALUE" ]; then
        echo -e "   $replica: ${GREEN}✅ Datos replicados correctamente${NC}"
    else
        echo -e "   $replica: ${RED}❌ Datos NO replicados${NC}"
    fi
done

echo ""
echo "============================================="
echo "  Health Check Completado"
echo "============================================="
