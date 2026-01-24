@echo off
REM ===========================================
REM MySQL Replication Health Check - Windows
REM ===========================================
echo.
echo =============================================
echo   MySQL Replication Health Check
echo =============================================
echo.

echo --- MASTER (mysql-writer) ---
docker exec -i mysql-writer mysql -uroot -pRootP@ssw0rd!2024 -e "SHOW MASTER STATUS\G" 2>nul | findstr /i "File Position"
echo.

echo --- REPLICA 1 (mysql-replica1) ---
docker exec -i mysql-replica1 mysql -uroot -pRootP@ssw0rd!2024 -e "SHOW SLAVE STATUS\G" 2>nul | findstr /i "Master_Host Slave_IO_Running Slave_SQL_Running Seconds_Behind Last_IO_Error Last_SQL_Error"
echo.

echo --- REPLICA 2 (mysql-replica2) ---
docker exec -i mysql-replica2 mysql -uroot -pRootP@ssw0rd!2024 -e "SHOW SLAVE STATUS\G" 2>nul | findstr /i "Master_Host Slave_IO_Running Slave_SQL_Running Seconds_Behind Last_IO_Error Last_SQL_Error"
echo.

echo =============================================
echo   Test de Replicacion
echo =============================================
echo.

echo Insertando datos de prueba en Master...
docker exec -i mysql-writer mysql -uroot -pRootP@ssw0rd!2024 fileshare_db -e "CREATE TABLE IF NOT EXISTS replication_test (id INT AUTO_INCREMENT PRIMARY KEY, test_value VARCHAR(100), created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP); INSERT INTO replication_test (test_value) VALUES ('Test %date% %time%');"
echo.

timeout /t 2 /nobreak >nul

echo Verificando datos en Writer:
docker exec -i mysql-writer mysql -uroot -pRootP@ssw0rd!2024 fileshare_db -e "SELECT * FROM replication_test ORDER BY id DESC LIMIT 1;"
echo.

echo Verificando datos en Replica 1:
docker exec -i mysql-replica1 mysql -uroot -pRootP@ssw0rd!2024 fileshare_db -e "SELECT * FROM replication_test ORDER BY id DESC LIMIT 1;"
echo.

echo Verificando datos en Replica 2:
docker exec -i mysql-replica2 mysql -uroot -pRootP@ssw0rd!2024 fileshare_db -e "SELECT * FROM replication_test ORDER BY id DESC LIMIT 1;"
echo.

echo =============================================
echo   Health Check Completado
echo =============================================
pause
