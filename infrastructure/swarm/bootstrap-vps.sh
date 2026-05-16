#!/usr/bin/env bash
# Run this FROM YOUR LOCAL MACHINE to bootstrap the Hostinger VPS.
# Usage: bash bootstrap-vps.sh
# It will SSH into the VPS, install Docker, init Swarm, copy configs, and deploy the stack.
set -euo pipefail

VPS_IP="177.7.48.169"
VPS_USER="root"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
INFRA_DIR="$(dirname "$SCRIPT_DIR")"

echo "=== TuColmadoRD — VPS Bootstrap ==="
echo "Target: $VPS_USER@$VPS_IP"
echo ""

# ── Step 1: install Docker + init Swarm ───────────────────────────────────────
echo ">>> Step 1: Installing Docker and initialising Swarm..."
ssh "$VPS_USER@$VPS_IP" bash <<'REMOTE'
set -euo pipefail
apt-get update -qq
apt-get install -y -qq docker.io curl git

# Docker Compose plugin
mkdir -p /usr/lib/docker/cli-plugins
if [[ ! -f /usr/lib/docker/cli-plugins/docker-compose ]]; then
  curl -SL "https://github.com/docker/compose/releases/download/v2.29.0/docker-compose-linux-x86_64" \
    -o /usr/lib/docker/cli-plugins/docker-compose
  chmod +x /usr/lib/docker/cli-plugins/docker-compose
fi

systemctl enable --now docker

# Init Swarm (idempotent)
if ! docker info 2>/dev/null | grep -q "Swarm: active"; then
  ADVERTISE_IP=$(hostname -I | awk '{print $1}')
  docker swarm init --advertise-addr "$ADVERTISE_IP"
  echo "Swarm initialised on $ADVERTISE_IP"
else
  echo "Swarm already active"
fi

# Create overlay networks
for net in traefik-public catalog-net reports-net app-net monitoring-net; do
  if ! docker network ls --format "{{.Name}}" | grep -q "^${net}$"; then
    docker network create --driver overlay --attachable "$net"
    echo "Created network: $net"
  fi
done

mkdir -p /opt/tucolmadord/letsencrypt
touch /opt/tucolmadord/letsencrypt/acme.json
chmod 600 /opt/tucolmadord/letsencrypt/acme.json
mkdir -p /opt/tucolmadord/monitoring
REMOTE

echo ">>> Step 1 complete."

# ── Step 2: copy infrastructure files ─────────────────────────────────────────
echo ">>> Step 2: Copying Swarm stack and monitoring configs..."
scp "$SCRIPT_DIR/docker-compose.swarm.yml" "$VPS_USER@$VPS_IP:/opt/tucolmadord/"
scp -r "$INFRA_DIR/monitoring" "$VPS_USER@$VPS_IP:/opt/tucolmadord/"
echo ">>> Step 2 complete."

# ── Step 3: create .env if it doesn't exist ───────────────────────────────────
echo ">>> Step 3: Checking .env..."
ENV_EXISTS=$(ssh "$VPS_USER@$VPS_IP" '[[ -f /opt/tucolmadord/.env ]] && echo yes || echo no')

if [[ "$ENV_EXISTS" == "no" ]]; then
  echo ""
  echo "⚠️  /opt/tucolmadord/.env not found on the VPS."
  echo "    Please create it with the following content (replace CHANGE_ME values):"
  echo ""
  cat "$SCRIPT_DIR/.env.example"
  echo ""
  echo "    You can do this by running:"
  echo "    ssh $VPS_USER@$VPS_IP 'cat > /opt/tucolmadord/.env' << 'EOF'"
  echo "    ... paste variables here ..."
  echo "    EOF"
  echo ""
  echo "    Then re-run this script to deploy the stack."
  exit 0
fi

echo ">>> .env found."

# ── Step 4: deploy Swarm stack ────────────────────────────────────────────────
echo ">>> Step 4: Deploying Swarm stack..."
ssh "$VPS_USER@$VPS_IP" bash <<'REMOTE'
set -euo pipefail
cd /opt/tucolmadord
set -a; source .env; set +a
docker stack deploy -c docker-compose.swarm.yml tucolmadord \
  --with-registry-auth --prune
REMOTE

echo ""
echo "=== Deployment complete! ==="
echo ""
echo "Checking service status in 15 seconds..."
sleep 15
ssh "$VPS_USER@$VPS_IP" docker service ls --filter label=com.docker.stack.namespace=tucolmadord
