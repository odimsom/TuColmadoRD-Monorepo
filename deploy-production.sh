#!/bin/bash
# Production initialization script for TuColmadoRD

set -e

echo "🚀 Starting TuColmadoRD production deployment..."

cd /app/tucolmadord

# 1. Pull latest code
echo "📦 Pulling latest code from main branch..."
git pull origin main --rebase

# 2. Update submodules
echo "📦 Updating submodules..."
git submodule update --recursive --init

# 3. Ensure .env exists and has required variables
if [ ! -f .env ]; then
    echo "❌ .env file not found. Please create .env with required variables."
    exit 1
fi

# 4. Clean up old containers and orphaned services
echo "🧹 Cleaning up old Docker artifacts..."
docker compose down --remove-orphans 2>/dev/null || true

# 5. Handle volume migration from Named to Bind-mounts
echo "📦 Migrating volumes to bind-mount configuration..."

# Remove ALL tucolmadord-related Named volumes (more aggressive cleanup)
echo "  Removing conflicting Named volumes..."
docker volume ls -q | grep -E "^tucolmadord" | while read vol; do
  echo "    Removing volume: $vol"
  docker volume rm "$vol" 2>/dev/null || true
done

# Force remove any lingering volumes with docker volume prune
docker volume prune -f --filter "label!=keep" 2>/dev/null || true

# Ensure bind-mount directories exist with proper permissions
echo "  Creating bind-mount directories..."
mkdir -p /var/lib/tucolmadord/postgres_data
mkdir -p /var/lib/tucolmadord/mongo_data
chmod 755 /var/lib/tucolmadord
chmod 755 /var/lib/tucolmadord/postgres_data
chmod 755 /var/lib/tucolmadord/mongo_data
echo "  ✅ Bind-mount directories ready"

# System cleanup (without --volumes, protecting any data in bind-mounts)
echo "  Running system prune..."
docker system prune -f 2>/dev/null || true

# 6. Run docker compose with rebuild
echo "🐳 Rebuilding and starting Docker services..."
docker compose pull
docker compose up --build -d

# 7. Wait for databases to be ready
echo "⏳ Waiting for databases to be ready..."
for i in {1..30}; do
  if docker compose exec -T postgres pg_isready -U ${POSTGRES_USER} > /dev/null 2>&1; then
    echo "✅ PostgreSQL is ready"
    break
  fi
  echo "  Waiting for PostgreSQL... ($i/30)"
  sleep 2
done

for i in {1..30}; do
  if docker compose exec -T mongo mongosh --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
    echo "✅ MongoDB is ready"
    break
  fi
  echo "  Waiting for MongoDB... ($i/30)"
  sleep 2
done

# 8. Wait for application services
echo "⏳ Waiting for application services to stabilize..."
sleep 15

# 9. Run database migrations
echo "🗄️ Running EF Core migrations for .NET API..."
docker compose exec -T api dotnet ef database update \
    --project src/infrastructure/TuColmadoRD.Infrastructure.Persistence \
    --startup-project src/Presentations/TuColmadoRD.Presentation.API \
    --configuration Release || {
    echo "⚠️  Migration warning: Check API logs with: docker compose logs api"
}

# 10. Verify all containers are healthy
echo "✅ Verifying service health..."
docker compose ps

# 11. Health check loop
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
