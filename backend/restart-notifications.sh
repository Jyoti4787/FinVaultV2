#!/bin/bash
# Restart notification service with new email credentials

echo "Stopping notification service..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml stop notification-service

echo "Removing notification service container..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml rm -f notification-service

echo "Starting notification service with new credentials..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml --env-file .env up -d notification-service

echo "Waiting for service to start..."
sleep 5

echo "Checking environment variables..."
docker exec finvault-notifications env | grep -i email

echo ""
echo "Done! Check logs with: docker logs finvault-notifications --tail 20"
