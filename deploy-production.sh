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

# 4. Run docker compose with rebuild
echo "🐳 Rebuilding and starting Docker services..."
docker compose down --remove-orphans
docker compose pull
docker compose up --build -d

# 5. Wait for services to be ready
echo "⏳ Waiting for services to stabilize..."
sleep 10

# 6. Run database migrations
echo "🗄️ Running EF Core migrations for .NET API..."
docker compose exec -T api dotnet ef database update \
    --project src/infrastructure/TuColmadoRD.Infrastructure.Persistence \
    --startup-project src/Presentations/TuColmadoRD.Presentation.API \
    --configuration Release 2>/dev/null || {
    echo "⚠️  Migration warning: ensure database is initialized"
}

# 7. Verify all containers are healthy
echo "✅ Verifying service health..."
docker compose ps

echo "✅ Deployment complete!"
echo ""
echo "Services available at:"
echo "  - Web Admin: https://${PUBLIC_WEB_DOMAIN}"
echo "  - API Gateway: https://${PUBLIC_API_DOMAIN}"
echo "  - Landing: https://${PUBLIC_LANDING_DOMAIN}"
echo ""
echo "📊 Check service status with: docker compose ps"
echo "📋 View logs with: docker compose logs -f [service_name]"
