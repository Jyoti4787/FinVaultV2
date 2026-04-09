#!/bin/bash
# stop-backend.sh

# Get the directory of this script so it can be run from anywhere
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" &> /dev/null && pwd)"

echo "🛑 Stopping FinVault Backend Services..."

# Navigate to the backend directory where docker-compose.yml lives
cd "$SCRIPT_DIR/backend" || { echo "❌ Failed to locate backend directory"; exit 1; }

# Stop the docker containers (preserves containers and networks)
docker compose stop "$@"

echo ""
echo "✅ Backend Services stopped successfully!"
