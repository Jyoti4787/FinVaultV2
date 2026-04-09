@echo off
REM FinVault One-Command Deployment Script for Windows
REM Usage: deploy.bat

setlocal enabledelayedexpansion

echo.
echo ===================================================
echo   FinVault - Complete Services Deployment
echo ===================================================
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if !errorlevel! neq 0 (
    echo [ERROR] Docker is not installed or not in PATH
    exit /b 1
)

REM Check if docker-compose is available
docker-compose --version >nul 2>&1
if !errorlevel! neq 0 (
    echo [ERROR] docker-compose is not installed
    exit /b 1
)

echo [1/7] Starting Infrastructure (SQL Server, RabbitMQ, SonarQube)...
docker-compose -f docker-compose.infra.yml up -d
if !errorlevel! neq 0 goto error

echo.
echo [2/7] Waiting for infrastructure to be ready (30 seconds)...
timeout /t 30 /nobreak

echo.
echo [3/7] Creating databases...
docker exec finvault-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Jyoti@Strong123" -Q "CREATE DATABASE [finvault_identity]; CREATE DATABASE [finvault_users]; CREATE DATABASE [finvault_cards]; CREATE DATABASE [finvault_payments]; CREATE DATABASE [finvault_transactions]; CREATE DATABASE [finvault_notifications]; CREATE DATABASE [finvault_rewards]; CREATE DATABASE [finvault_support];" 2>nul

echo.
echo [4/7] Building all microservices (this may take 2-5 minutes)...
docker-compose build --no-cache
if !errorlevel! neq 0 goto error

echo.
echo [5/7] Starting all microservices...
docker-compose up -d
if !errorlevel! neq 0 goto error

echo.
echo [6/7] Waiting for services to start (30 seconds)...
timeout /t 30 /nobreak

echo.
echo [7/7] Verifying all services...
docker-compose ps

echo.
echo ===================================================
echo   Deployment Complete!
echo ===================================================
echo.
echo Service Access Information:
echo.
echo Identity Service (Auth):     http://localhost:5232/swagger
echo User Service:                 http://localhost:5261/swagger
echo Card Service:                 http://localhost:5121/swagger
echo Payment Service:              http://localhost:5181/swagger
echo Transaction Service (Saga):   http://localhost:5171/swagger
echo Notification Service:         http://localhost:5191/swagger
echo Reward Service:               http://localhost:5276/swagger
echo Support Service:              http://localhost:5281/swagger
echo API Gateway (Ocelot):         http://localhost:5001/health
echo.
echo Monitoring:
echo RabbitMQ:                    http://localhost:15672 (guest/guest)
echo SonarQube:                   http://localhost:9000 (admin/admin)
echo.
goto :end

:error
echo.
echo [ERROR] Deployment failed!
exit /b 1

:end
endlocal
