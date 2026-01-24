#!/bin/bash
# ===========================================
# MySQL Replication Setup Script
# Este script configura la replicación Master-Slave
# ===========================================

set -e

echo "============================================="
echo "  MySQL Replication Setup Script"
echo "============================================="

# Variables
MASTER_HOST="mysql-writer"
MASTER_PORT=3306
REPLICA1_HOST="mysql-replica1"
REPLICA2_HOST="mysql-replica2"
ROOT_PASSWORD="${MYSQL_ROOT_PASSWORD:-RootP@ssw0rd!2024}"
REPL_USER="repl_user"
REPL_PASSWORD="repl_pass123"

# Función para esperar que MySQL esté disponible
wait_for_mysql() {
    local host=$1
    local max_attempts=30
    local attempt=0
    
    echo "⏳ Esperando a que $host esté disponible..."
    while [ $attempt -lt $max_attempts ]; do
        if mysqladmin ping -h "$host" -uroot -p"$ROOT_PASSWORD" --silent 2>/dev/null; then
            echo "✅ $host está disponible"
            return 0
        fi
        attempt=$((attempt + 1))
        sleep 2
    done
    echo "❌ Timeout esperando a $host"
    return 1
}

# Función para configurar una réplica
setup_replica() {
    local replica_host=$1
    local replica_name=$2
    
    echo ""
    echo "🔧 Configurando $replica_name ($replica_host)..."
    
    # Obtener posición actual del master
    echo "📍 Obteniendo posición del binary log del Master..."
    MASTER_STATUS=$(mysql -h "$MASTER_HOST" -uroot -p"$ROOT_PASSWORD" -e "SHOW MASTER STATUS\G" 2>/dev/null)
    MASTER_LOG_FILE=$(echo "$MASTER_STATUS" | grep "File:" | awk '{print $2}')
    MASTER_LOG_POS=$(echo "$MASTER_STATUS" | grep "Position:" | awk '{print $2}')
    
    if [ -z "$MASTER_LOG_FILE" ] || [ -z "$MASTER_LOG_POS" ]; then
        echo "❌ No se pudo obtener la posición del Master"
        return 1
    fi
    
    echo "   Log File: $MASTER_LOG_FILE"
    echo "   Position: $MASTER_LOG_POS"
    
    # Verificar si la réplica ya está configurada
    SLAVE_STATUS=$(mysql -h "$replica_host" -uroot -p"$ROOT_PASSWORD" -e "SHOW SLAVE STATUS\G" 2>/dev/null)
    SLAVE_IO=$(echo "$SLAVE_STATUS" | grep "Slave_IO_Running:" | awk '{print $2}')
    SLAVE_SQL=$(echo "$SLAVE_STATUS" | grep "Slave_SQL_Running:" | awk '{print $2}')
    
    if [ "$SLAVE_IO" = "Yes" ] && [ "$SLAVE_SQL" = "Yes" ]; then
        echo "✅ $replica_name ya está configurada y funcionando"
        return 0
    fi
    
    echo "📦 Creando dump del Master..."
    mysqldump -h "$MASTER_HOST" -uroot -p"$ROOT_PASSWORD" \
        --all-databases \
        --master-data=2 \
        --single-transaction \
        --routines \
        --triggers \
        2>/dev/null > /tmp/master_dump.sql
    
    echo "📥 Importando dump en $replica_name..."
    mysql -h "$replica_host" -uroot -p"$ROOT_PASSWORD" -e "STOP SLAVE; RESET SLAVE ALL;" 2>/dev/null
    mysql -h "$replica_host" -uroot -p"$ROOT_PASSWORD" < /tmp/master_dump.sql 2>/dev/null
    
    # Obtener nueva posición después del dump
    NEW_MASTER_STATUS=$(mysql -h "$MASTER_HOST" -uroot -p"$ROOT_PASSWORD" -e "SHOW MASTER STATUS\G" 2>/dev/null)
    NEW_MASTER_LOG_FILE=$(echo "$NEW_MASTER_STATUS" | grep "File:" | awk '{print $2}')
    NEW_MASTER_LOG_POS=$(echo "$NEW_MASTER_STATUS" | grep "Position:" | awk '{print $2}')
    
    echo "🔗 Configurando replicación..."
    mysql -h "$replica_host" -uroot -p"$ROOT_PASSWORD" -e "
        CHANGE MASTER TO
            MASTER_HOST='$MASTER_HOST',
            MASTER_PORT=$MASTER_PORT,
            MASTER_USER='$REPL_USER',
            MASTER_PASSWORD='$REPL_PASSWORD',
            MASTER_LOG_FILE='$NEW_MASTER_LOG_FILE',
            MASTER_LOG_POS=$NEW_MASTER_LOG_POS;
        START SLAVE;
    " 2>/dev/null
    
    # Verificar estado
    sleep 2
    FINAL_STATUS=$(mysql -h "$replica_host" -uroot -p"$ROOT_PASSWORD" -e "SHOW SLAVE STATUS\G" 2>/dev/null)
    FINAL_IO=$(echo "$FINAL_STATUS" | grep "Slave_IO_Running:" | awk '{print $2}')
    FINAL_SQL=$(echo "$FINAL_STATUS" | grep "Slave_SQL_Running:" | awk '{print $2}')
    FINAL_LAG=$(echo "$FINAL_STATUS" | grep "Seconds_Behind_Master:" | awk '{print $2}')
    
    if [ "$FINAL_IO" = "Yes" ] && [ "$FINAL_SQL" = "Yes" ]; then
        echo "✅ $replica_name configurada exitosamente!"
        echo "   Slave_IO_Running: $FINAL_IO"
        echo "   Slave_SQL_Running: $FINAL_SQL"
        echo "   Seconds_Behind_Master: $FINAL_LAG"
    else
        echo "❌ Error configurando $replica_name"
        LAST_ERROR=$(echo "$FINAL_STATUS" | grep "Last_IO_Error:" | cut -d':' -f2-)
        echo "   Last_IO_Error: $LAST_ERROR"
        return 1
    fi
    
    # Limpiar
    rm -f /tmp/master_dump.sql
}

# ===========================================
# MAIN
# ===========================================

echo ""
echo "🚀 Iniciando configuración de replicación..."
echo ""

# Esperar a que todos los servidores estén disponibles
wait_for_mysql "$MASTER_HOST"
wait_for_mysql "$REPLICA1_HOST"
wait_for_mysql "$REPLICA2_HOST"

# Configurar réplicas
setup_replica "$REPLICA1_HOST" "Replica 1"
setup_replica "$REPLICA2_HOST" "Replica 2"

echo ""
echo "============================================="
echo "  ✅ Configuración de replicación completada"
echo "============================================="
echo ""
echo "📊 Estado final de la replicación:"
echo ""

# Mostrar estado final
for replica in "$REPLICA1_HOST" "$REPLICA2_HOST"; do
    echo "--- $replica ---"
    mysql -h "$replica" -uroot -p"$ROOT_PASSWORD" -e "
        SHOW SLAVE STATUS\G" 2>/dev/null | grep -E "Slave_IO_Running|Slave_SQL_Running|Seconds_Behind_Master|Master_Host"
    echo ""
done

echo "🎉 ¡Listo! La replicación está funcionando."
