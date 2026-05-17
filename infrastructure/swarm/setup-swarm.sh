#!/usr/bin/env bash
# Run once on the Hostinger VPS to initialise Docker Swarm and deploy the stack.
# Usage: bash setup-swarm.sh
set -euo pipefail

APP_DIR=/opt/tucolmadord
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "=== TuColmadoRD — Swarm bootstrap ==="

# 1. Ensure Docker is installed
if ! command -v docker &>/dev/null; then
  echo "Installing Docker..."
  apt-get update -qq && apt-get install -y -qq docker.io
  systemctl enable --now docker
fi

# 2. Init Swarm (idempotent)
if ! docker info 2>/dev/null | grep -q "Swarm: active"; then
  ADVERTISE_IP=$(hostname -I | awk '{print $1}')
  docker swarm init --advertise-addr "$ADVERTISE_IP"
  echo "Swarm initialised on $ADVERTISE_IP"
else
  echo "Swarm already active"
fi

# 3. Create overlay networks (idempotent)
for net in traefik-public catalog-net reports-net app-net monitoring-net; do
  if ! docker network ls | grep -q "$net"; then
    docker network create --driver overlay --attachable "$net"
    echo "Created network: $net"
  fi
done

# 4. Create app directory
mkdir -p "$APP_DIR/letsencrypt"
touch "$APP_DIR/letsencrypt/acme.json"
chmod 600 "$APP_DIR/letsencrypt/acme.json"

# 5. Copy configs
cp "$SCRIPT_DIR/docker-compose.swarm.yml" "$APP_DIR/"
cp -r "$SCRIPT_DIR/../monitoring" "$APP_DIR/"

# 6. Ensure .env exists (operator must have created it)
if [[ ! -f "$APP_DIR/.env" ]]; then
  echo ""
  echo "ERROR: $APP_DIR/.env not found."
  echo "Create it with the following variables:"
  cat << 'EOF'
DATABASE_URL=postgresql://user:pass@postgres:5432/tucolmadord
POSTGRES_USER=tucolmado
POSTGRES_PASSWORD=<strong-password>
POSTGRES_DB=tucolmadord
REDIS_URL=redis://redis:6379
DOMAIN=tucolmadord.com
ACME_EMAIL=admin@tucolmadord.com
GRAFANA_PASSWORD=<strong-password>
ALERT_WEBHOOK_URL=https://hooks.slack.com/...
REGISTRY=ghcr.io/synsetsolutions
TAG=latest
EOF
  exit 1
fi

# 7. Log in to GHCR (needs GITHUB_TOKEN env var or interactive)
if [[ -n "${GITHUB_TOKEN:-}" ]]; then
  echo "$GITHUB_TOKEN" | docker login ghcr.io -u github-actions --password-stdin
fi

# 8. Deploy stack
cd "$APP_DIR"
set -a; source .env; set +a
docker stack deploy -c docker-compose.swarm.yml tucolmadord --with-registry-auth --prune

echo ""
echo "=== Stack deployed ==="
docker service ls --filter label=com.docker.stack.namespace=tucolmadord
