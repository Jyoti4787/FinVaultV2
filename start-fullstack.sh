#!/bin/bash

# FinVault Full Stack Startup Script
# This script starts both backend and frontend services

echo " Starting FinVault Full Stack..."
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Step 1: Start Backend Services
echo -e "${BLUE} Starting Backend Services...${NC}"
cd backend
docker-compose up -d

# Wait for services to be healthy
echo -e "${YELLOW}⏳ Waiting for services to be ready...${NC}"
sleep 5

# Check if services are running
if docker ps | grep -q "finvault-gateway"; then
    echo -e "${GREEN}✅ Backend services are running${NC}"
    echo ""
    echo "Backend Services:"
    echo "  - Gateway:        http://localhost:5001"
    echo "  - Swagger:        http://localhost:5001/swagger"
    echo "  - Identity:       http://localhost:5232"
    echo "  - Cards:          http://localhost:5121"
    echo "  - Payments:       http://localhost:5181"
    echo "  - Notifications:  http://localhost:5191"
    echo "  - RabbitMQ:       http://localhost:15672"
    echo ""
else
    echo -e "${YELLOW}  Backend services may not be fully started yet${NC}"
    echo "Run 'docker ps' to check status"
    echo ""
fi

# Step 2: Start Frontend
cd ../frontend
echo -e "${BLUE} Starting Frontend...${NC}"
echo ""
echo -e "${GREEN}Frontend will be available at: http://localhost:4200${NC}"
echo ""
echo -e "${YELLOW}Test Credentials:${NC}"
echo "  User:  ankit.robin@example.com / Ankit@123"
echo "  Admin: admin@finvault.io / Admin@123"
echo ""
echo -e "${BLUE}Starting Angular dev server...${NC}"
echo ""

# Start frontend (this will block)
ng serve 
