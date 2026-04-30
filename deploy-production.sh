#!/bin/bash
# Production initialization script for TuColmadoRD

set -e

# Check if running as root (required for volume management)
if [ "$EUID" -ne 0 ]; then
  echo "❌ Error: This script must be run as root"
  echo ""
  echo "This script requires root access to:"
  echo "  - Create /var/lib/tucolmadord directories"
  echo "  - Manage Docker volumes and containers"
  echo ""
  echo "Setup Options:"
  echo ""
  echo "Option 1 (RECOMMENDED): Configure sudo without password on VPS"
  echo "  Run this on the VPS as root:"
  echo "    echo 'github-actions ALL=(ALL) NOPASSWD: ALL' | sudo tee /etc/sudoers.d/github-actions"
  echo "    sudo chmod 0440 /etc/sudoers.d/github-actions"
  echo ""
  echo "Option 2: Use GitHub Actions secret for sudo password"
  echo "  1. Create a secret in GitHub: Settings > Secrets > VPS_SUDO_PASSWORD"
  echo "  2. Workflow will use: echo \$SUDO_PASSWORD | sudo -S bash deploy-production.sh"
  echo ""
  exit 1
fi

echo "🚀 Starting TuColmadoRD production deployment..."

cd /app/tucolmadord

# 1. Prepare git (stash any local changes and pull latest code)
echo "📦 Preparing git repository..."
git stash || true  # Stash local changes if they exist
echo "📦 Pulling latest code from main branch..."
git fetch origin
git reset --hard origin/main  # Hard reset to ensure we have the latest code

# 2. Update submodules (non-critical - continue if fails)
echo "📦 Updating submodules..."
# First, try to deinit any corrupted submodules
git submodule deinit -f --all 2>/dev/null || true

# Then reinitialize and update
git submodule update --recursive --init 2>/dev/null || {
  echo "⚠️  Warning: Submodule update had issues. Attempting force init..."
  
  # Manual fix: remove problematic submodule and reinit
  rm -rf .git/modules/generadordexmle-cf 2>/dev/null || true
  rm -rf generadordexmle-cf 2>/dev/null || true
  
  # Try once more
  git submodule update --recursive --init 2>/dev/null || {
    echo "⚠️  Warning: Submodule init failed (non-critical, continuing...)"
    # Submodule failures are non-blocking since services use Docker images
  }
}

# 3. Verify .env exists and load variables (rsync preserves it from previous deployments)
if [ ! -f .env ]; then
    echo "❌ CRITICAL ERROR: .env file not found in /app/tucolmadord"
    echo ""
    echo "This means either:"
    echo "  1. First deployment - .env not created yet"
    echo "  2. Someone deleted it"
    echo "  3. rsync excluded it on deployment"
    echo ""
    echo "To fix: Create .env on the VPS with production credentials:"
    echo "  ssh root@177.7.48.169"
    echo "  cd /app/tucolmadord"
    echo "  nano .env"
    echo ""
    echo "Reference: See .env.example in repository for format"
    echo ""
    exit 1
else
    echo "✅ .env file found"
    # Load environment variables from .env so they're available to this script
    set -a  # Export all variable assignments
    source .env
    set +a  # Stop exporting
fi

# 4. Aggressive cleanup of existing containers and volumes
echo "🧹 Aggressive cleanup of old Docker artifacts..."

# 4a. Stop and remove ALL containers (force if needed)
echo "  Stopping and removing all tucolmadord containers..."
docker ps -q --filter "label=com.docker.compose.project=tucolmadord" | xargs -r docker rm -f 2>/dev/null || true
docker compose down --remove-orphans 2>/dev/null || true

# 4b. Remove ALL tucolmadord Named volumes explicitly (with force if they're in use)
echo "  Removing Named volumes..."
docker volume ls -q | grep -E "^tucolmadord" | while read vol; do
  echo "    Forcing removal: $vol"
  docker volume rm -f "$vol" 2>/dev/null || true
done

# 4c. Clean up bind-mount directories on filesystem (aggressive)
echo "  Cleaning bind-mount directories..."
if [ -d /var/lib/tucolmadord ]; then
  rm -rf /var/lib/tucolmadord/postgres_data 2>/dev/null || true
  rm -rf /var/lib/tucolmadord/mongo_data 2>/dev/null || true
fi

# 5. Prune all unused Docker objects
echo "  Pruning unused Docker objects..."
docker system prune -f 2>/dev/null || true
docker volume prune -f 2>/dev/null || true

# 5b. Resolve port conflicts (Port 80/443)
echo "🧹 Resolving port conflicts for 80/443..."
# Stop host-level services if they exist
systemctl stop nginx 2>/dev/null || true
systemctl stop apache2 2>/dev/null || true
systemctl stop traefik 2>/dev/null || true

# Find and stop any docker container using port 80 (not just our project)
CONFLICTING_CONTAINERS=$(docker ps -q --filter "publish=80")
if [ ! -z "$CONFLICTING_CONTAINERS" ]; then
  echo "  Stopping conflicting containers: $CONFLICTING_CONTAINERS"
  docker stop $CONFLICTING_CONTAINERS || true
fi

# 6. Create fresh bind-mount directories with correct permissions
echo "📦 Setting up fresh bind-mount directories..."
mkdir -p /var/lib/tucolmadord/postgres_data
mkdir -p /var/lib/tucolmadord/mongo_data
mkdir -p letsencrypt
chmod 755 /var/lib/tucolmadord
chmod 755 /var/lib/tucolmadord/postgres_data
chmod 755 /var/lib/tucolmadord/mongo_data
chmod 700 letsencrypt
echo "  ✅ Bind-mount directories ready"

# 7. Run docker compose with rebuild
echo "🐳 Rebuilding and starting Docker services..."
docker compose pull
docker compose up --build -d

# 8. Wait for databases to be ready
echo "⏳ Waiting for databases to be ready..."
POSTGRES_READY=false
for i in {1..30}; do
  if docker compose exec -T postgres pg_isready -U ${POSTGRES_USER} > /dev/null 2>&1; then
    echo "✅ PostgreSQL is ready"
    POSTGRES_READY=true
    break
  fi
  echo "  Waiting for PostgreSQL... ($i/30)"
  sleep 2
done

if [ "$POSTGRES_READY" = false ]; then
  echo "⚠️  Warning: PostgreSQL may not be ready, but continuing..."
fi

MONGO_READY=false
for i in {1..30}; do
  if docker compose exec -T mongo mongosh --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
    echo "✅ MongoDB is ready"
    MONGO_READY=true
    break
  fi
  echo "  Waiting for MongoDB... ($i/30)"
  sleep 2
done

if [ "$MONGO_READY" = false ]; then
  echo "⚠️  Warning: MongoDB may not be ready, but continuing..."
fi

# 9. Wait for application services
echo "⏳ Waiting for application services to stabilize..."
sleep 15

# 10. Run database migrations
echo "🗄️ Running EF Core migrations for .NET API..."
docker compose exec -T api dotnet ef database update \
    --project src/infrastructure/TuColmadoRD.Infrastructure.Persistence \
    --startup-project src/Presentations/TuColmadoRD.Presentation.API \
    --configuration Release || {
    echo "⚠️  Migration warning: Check API logs with: docker compose logs api"
}

# 11. Verify all containers are healthy
echo "✅ Verifying service health..."
docker compose ps

# 12. Health check loop
echo "🏥 Waiting for all services to be healthy..."
MAX_ATTEMPTS=20
ATTEMPT=0
while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
  UNHEALTHY=$(docker compose ps --format json | grep -c '"Health":"unhealthy"' || true)
  STARTING=$(docker compose ps --format json | grep -c '"Health":"starting"' || true)
  
  if [ $UNHEALTHY -eq 0 ] && [ $STARTING -eq 0 ]; then
    echo "✅ All services are healthy!"
    break
  fi
  
  ATTEMPT=$((ATTEMPT+1))
  echo "  Health check $ATTEMPT/$MAX_ATTEMPTS - Unhealthy: $UNHEALTHY, Starting: $STARTING"
  sleep 5
done

if [ $ATTEMPT -eq $MAX_ATTEMPTS ]; then
  echo "⚠️  Some services may not be healthy. Check logs with: docker compose logs"
fi

echo ""
echo "✅ Deployment complete!"
echo ""
echo "Services available at:"
echo "  - Web Admin: https://${PUBLIC_WEB_DOMAIN}"
echo "  - API Gateway: https://${PUBLIC_API_DOMAIN}"
echo "  - Landing: https://${PUBLIC_LANDING_DOMAIN}"
echo ""
echo "📊 Check service status with: docker compose ps"
echo "📋 View logs with: docker compose logs -f [service_name]"
