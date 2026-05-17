terraform {
  required_version = ">= 1.6"
  required_providers {
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.5"
    }
  }
  # Store state remotely — use S3-compatible (Backblaze B2 or AWS S3)
  # backend "s3" {
  #   bucket = "tucolmadord-tfstate"
  #   key    = "hostinger/terraform.tfstate"
  #   region = "us-east-1"
  # }
}

variable "vps_ip"       { description = "Hostinger VPS public IP" }
variable "ssh_user"     { default     = "root" }
variable "ssh_key_path" { description = "Local path to SSH private key" }
variable "domain"       { description = "Primary domain e.g. tucolmadord.com" }
variable "acme_email"   { description = "Email for Let's Encrypt" }

# ── Provision VPS via SSH ──────────────────────────────────────────────────────
resource "null_resource" "bootstrap" {
  connection {
    type        = "ssh"
    host        = var.vps_ip
    user        = var.ssh_user
    private_key = file(var.ssh_key_path)
    timeout     = "5m"
  }

  provisioner "remote-exec" {
    inline = [
      "apt-get update -qq",
      "apt-get install -y -qq docker.io docker-compose-plugin curl git",
      "systemctl enable --now docker",
      # Init Swarm if not already
      "docker info | grep -q 'Swarm: active' || docker swarm init --advertise-addr ${var.vps_ip}",
      # Create overlay networks
      "docker network ls | grep -q traefik-public || docker network create --driver overlay --attachable traefik-public",
      # Create app directory
      "mkdir -p /opt/tucolmadord/letsencrypt",
      "touch /opt/tucolmadord/letsencrypt/acme.json",
      "chmod 600 /opt/tucolmadord/letsencrypt/acme.json",
    ]
  }

  triggers = {
    vps_ip = var.vps_ip
  }
}

resource "null_resource" "deploy_stack" {
  depends_on = [null_resource.bootstrap]

  connection {
    type        = "ssh"
    host        = var.vps_ip
    user        = var.ssh_user
    private_key = file(var.ssh_key_path)
  }

  provisioner "file" {
    source      = "${path.module}/../../swarm/docker-compose.swarm.yml"
    destination = "/opt/tucolmadord/docker-compose.swarm.yml"
  }

  provisioner "file" {
    source      = "${path.module}/../../monitoring"
    destination = "/opt/tucolmadord/monitoring"
  }

  provisioner "remote-exec" {
    inline = [
      "cd /opt/tucolmadord && docker stack deploy -c docker-compose.swarm.yml tucolmadord --with-registry-auth"
    ]
  }

  triggers = {
    stack_hash = filemd5("${path.module}/../../swarm/docker-compose.swarm.yml")
  }
}

output "vps_ip" {
  value = var.vps_ip
}
