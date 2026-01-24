@echo off
REM ===========================================
REM MySQL Replication Setup - Windows Script
REM ===========================================
echo.
echo =============================================
echo   MySQL Replication Setup
echo =============================================
echo.

REM Verificar que Docker está corriendo
docker info >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo [ERROR] Docker no esta corriendo. Por favor inicia Docker Desktop.
    pause
    exit /b 1
)

echo [1/5] Verificando contenedores MySQL...
docker ps | findstr mysql-writer >nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] mysql-writer no esta corriendo.
    echo Por favor ejecuta: docker-compose up -d mysql-writer mysql-replica1 mysql-replica2
    pause
    exit /b 1
)

echo [2/5] Obteniendo posicion del Master...
for /f "tokens=*" %%a in ('docker exec mysql-writer sh -c "mysql -uroot -p%MYSQL_ROOT_PASSWORD% -N -e \"SHOW MASTER STATUS\" 2>/dev/null" ^| head -1') do set MASTER_INFO=%%a
echo Master Info: %MASTER_INFO%

echo [3/5] Configurando Replica 1...
docker exec mysql-writer sh -c "mysqldump -uroot -pRootP@ssw0rd!2024 --all-databases --master-data=2 --single-transaction 2>/dev/null | mysql -h mysql-replica1 -uroot -pRootP@ssw0rd!2024 2>/dev/null"

echo [4/5] Configurando Replica 2...
docker exec mysql-writer sh -c "mysqldump -uroot -pRootP@ssw0rd!2024 --all-databases --master-data=2 --single-transaction 2>/dev/null | mysql -h mysql-replica2 -uroot -pRootP@ssw0rd!2024 2>/dev/null"

echo [5/5] Iniciando replicacion...
REM Obtener posicion actual
for /f %%a in ('docker exec -i mysql-writer mysql -uroot -pRootP@ssw0rd!2024 -N -e "SHOW MASTER STATUS" 2^>nul ^| powershell -Command "$input | ForEach-Object { $_.Split()[0] }"') do set LOG_FILE=%%a
for /f %%a in ('docker exec -i mysql-writer mysql -uroot -pRootP@ssw0rd!2024 -N -e "SHOW MASTER STATUS" 2^>nul ^| powershell -Command "$input | ForEach-Object { $_.Split()[1] }"') do set LOG_POS=%%a

echo Log File: %LOG_FILE%
echo Log Position: %LOG_POS%

docker exec -i mysql-replica1 mysql -uroot -pRootP@ssw0rd!2024 -e "CHANGE MASTER TO MASTER_HOST='mysql-writer', MASTER_PORT=3306, MASTER_USER='repl_user', MASTER_PASSWORD='repl_pass123', MASTER_LOG_FILE='%LOG_FILE%', MASTER_LOG_POS=%LOG_POS%; START SLAVE;" 2>nul

docker exec -i mysql-replica2 mysql -uroot -pRootP@ssw0rd!2024 -e "CHANGE MASTER TO MASTER_HOST='mysql-writer', MASTER_PORT=3306, MASTER_USER='repl_user', MASTER_PASSWORD='repl_pass123', MASTER_LOG_FILE='%LOG_FILE%', MASTER_LOG_POS=%LOG_POS%; START SLAVE;" 2>nul

echo.
echo =============================================
echo   Verificando estado de la replicacion
echo =============================================
echo.

timeout /t 3 /nobreak >nul

echo --- Replica 1 ---
docker exec -i mysql-replica1 mysql -uroot -pRootP@ssw0rd!2024 -e "SHOW SLAVE STATUS\G" 2>nul | findstr /i "Slave_IO_Running Slave_SQL_Running Seconds_Behind"

echo.
echo --- Replica 2 ---
docker exec -i mysql-replica2 mysql -uroot -pRootP@ssw0rd!2024 -e "SHOW SLAVE STATUS\G" 2>nul | findstr /i "Slave_IO_Running Slave_SQL_Running Seconds_Behind"

echo.
echo =============================================
echo   Configuracion completada!
echo =============================================
echo.
pause
