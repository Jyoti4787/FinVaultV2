#!/bin/bash
# start-backend.sh

# Get the directory of this script so it can be run from anywhere
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

echo "🚀 Starting FinVault Backend Services..."

# Navigate to the backend directory where docker-compose.yml lives
cd "$SCRIPT_DIR/backend" || { echo "❌ Failed to locate backend directory"; exit 1; }

# Start the docker containers in detached mode
# If you want to force rebuild, you can pass --build to the script: ./start-backend.sh --build
docker compose up -d "$@"

echo ""
echo "✅ Backend Services Started successfully!"
echo "   Gateway Swagger UI is available at: http://localhost:5001/swagger"
