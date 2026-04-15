# FinVault - Project Setup Guide

## Prerequisites

- Docker Desktop installed and running
- Node.js (v18 or higher) and npm installed
- .NET 10.0 SDK (optional, only if running services outside Docker)

## Backend Setup

### 1. Start Infrastructure Services

Navigate to the backend directory and start SQL Server and RabbitMQ:

```bash
cd backend
docker-compose -f docker-compose.infra.yml up -d
```

Wait for services to be healthy (about 30 seconds):

```bash
docker-compose -f docker-compose.infra.yml ps
```

### 2. Initialize Database

The database will be automatically initialized when services start. To manually seed test data:

```bash
docker exec -it finvault-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd" \
  -i /docker-entrypoint-initdb.d/seed-dummy-data.sql -C
```

### 3. Start All Microservices

```bash
docker-compose up -d
```

### 4. Verify All Services Are Running

```bash
docker-compose ps
```

You should see all services with status "Up":
- finvault-gateway (Port 5001)
- finvault-identity (Port 5232)
- finvault-cards (Port 5121)
- finvault-payments (Port 5181)
- finvault-notifications (Port 5191)
- finvault-sqlserver (Port 1433)
- finvault-rabbitmq (Ports 5672, 15672)

### 5. Check Service Logs (if needed)

```bash
# View all logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f identity-service
docker-compose logs -f ocelot-gateway
```

### 6. Access Swagger Documentation

- Gateway Swagger: http://localhost:5001/swagger
- Identity Service: http://localhost:5232/swagger
- Card Service: http://localhost:5121/swagger
- Payment Service: http://localhost:5181/swagger
- Notification Service: http://localhost:5191/swagger

## Frontend Setup

### 1. Install Dependencies

```bash
cd frontend
npm install
```

### 2. Start Development Server

```bash
npm start
```

The frontend will be available at: http://localhost:4200

## Test Credentials

### User Account
- Email: `ankit.robin@example.com`
- Password: `Ankit@123`
- Has 2 credit cards, 3 payments, and 4 notifications

### Admin Account
- Email: `admin@finvault.io`
- Password: `Admin@123`
- Admin role (no test data)

## Valid Test Card Numbers

Use these Luhn-compliant card numbers when adding new cards:

- Visa: `4532123456789012`
- Mastercard: `5425233430109903` (recommended)
- RuPay: `6067874111055690` or `6540076749724922`
- Amex: `375551579726892`

## Common Issues & Solutions

### Services Not Starting

```bash
# Stop all services
docker-compose down

# Remove volumes and restart
docker-compose down -v
docker-compose up -d
```

### Database Connection Issues

```bash
# Restart SQL Server
docker-compose restart sqlserver

# Wait 30 seconds, then restart dependent services
docker-compose restart identity-service card-service payment-service
```

### Gateway 502/405 Errors

```bash
# Restart gateway
docker-compose restart ocelot-gateway
```

### Identity Service Crashes

```bash
# Check logs
docker logs finvault-identity

# Rebuild and restart
docker-compose build identity-service
docker-compose up -d identity-service
```

### Frontend Can't Connect to Backend

1. Verify backend gateway is running: http://localhost:5001/swagger
2. Check browser console for CORS errors
3. Clear localStorage and login again

## Stopping the Project

### Stop All Services

```bash
cd backend
docker-compose down
```

### Stop and Remove All Data

```bash
docker-compose down -v
```

## Development Workflow

### Rebuilding a Service After Code Changes

```bash
# Rebuild specific service
docker-compose build identity-service

# Restart the service
docker-compose up -d identity-service

# View logs
docker-compose logs -f identity-service
```

### Rebuilding All Services

```bash
docker-compose build
docker-compose up -d
```

## API Gateway Endpoints

All requests go through the gateway at `http://localhost:5001/api`

### Authentication (No Auth Required)
- POST `/api/identity/auth/register` - Register new user
- POST `/api/identity/auth/login` - Login
- POST `/api/identity/auth/verify-otp` - Verify OTP

### User Profile (Auth Required)
- GET `/api/identity/users/profile` - Get profile
- PUT `/api/identity/users/profile` - Update profile (email read-only)

### Cards (Auth Required)
- GET `/api/cards` - Get user's cards
- POST `/api/cards` - Add new card
- GET `/api/cards/{id}` - Get card details
- GET `/api/cards/{id}/reveal` - Reveal card number and CVV

### Payments (Auth Required)
- GET `/api/payments` - Get user's payments
- POST `/api/payments` - Process payment
- GET `/api/transactions` - Get transactions

### Notifications (Auth Required)
- GET `/api/notifications` - Get user's notifications
- PUT `/api/notifications/{id}/read` - Mark as read

## RabbitMQ Management

Access RabbitMQ management UI at: http://localhost:15672
- Username: `guest`
- Password: `guest`

## Notes

- Swagger UI requires manual "Bearer " prefix before token
- Only newly added cards (after encryption implementation) will reveal full number and CVV
- Email field in profile is read-only for security
- Phone number supports country code format (e.g., +919876543210)
