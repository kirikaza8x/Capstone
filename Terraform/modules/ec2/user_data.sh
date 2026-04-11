#!/bin/bash
# ============================================
# EC2 User Data — Amazon Linux 2023
# Installs: Docker, ECS Agent, Redis, RabbitMQ, Qdrant, Nginx, Certbot, Swap
# ============================================

set -euo pipefail

# --- Variables from Terraform template ---
PROJECT="${project}"
ENVIRONMENT="${environment}"
DOMAIN_NAME="${domain_name}"
RABBITMQ_USER="${rabbitmq_user}"
RABBITMQ_PASS="${rabbitmq_pass}"
REDIS_PASS="${redis_pass}"
BACKEND_PORT="${backend_port}"

# ============================================
# 1. Create 2GB Swap File (prevent OOM crashes)
# ============================================

echo "Creating 2GB swap file..."
dd if=/dev/zero of=/swapfile bs=128M count=16
chmod 600 /swapfile
mkswap /swapfile
swapon /swapfile
echo "/swapfile none swap sw 0 0" >> /etc/fstab

# ============================================
# 2. Install Docker + ECS Agent
# ============================================

echo "Installing Docker and ECS Agent..."
dnf install -y docker
systemctl start docker
systemctl enable docker

# Install ECS Agent
cat > /etc/ecs/ecs.config << EOF
ECS_CLUSTER=${PROJECT}-${ENVIRONMENT}-cluster
ECS_ENGINE_AUTH_TYPE=dockercfg
ECS_AVAILABLE_LOGGING_DRIVERS=["json-file","awslogs"]
ECS_LOGLEVEL=info
ECS_CONTAINER_INSTANCE_TAGS={"Project": "${PROJECT}", "Environment": "${ENVIRONMENT}"}
EOF

systemctl start ecs
systemctl enable ecs

# ============================================
# 3. Install Docker Compose (for Redis, RabbitMQ, Qdrant)
# ============================================

echo "Installing Docker Compose..."
DOCKER_COMPOSE_VERSION="v2.24.0"
curl -L "https://github.com/docker/compose/releases/download/${DOCKER_COMPOSE_VERSION}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# ============================================
# 4. Create Infrastructure Docker Compose (Redis, RabbitMQ, Qdrant)
# ============================================

echo "Creating infrastructure containers..."
mkdir -p /opt/aipromo

cat > /opt/aipromo/docker-compose.yml << COMPOSE_EOF
version: '3.8'

services:
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: rabbitmq
    restart: always
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASS}
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - aipromo-network
    mem_limit: 400m

  redis:
    image: redis:7-alpine
    container_name: redis
    restart: always
    ports:
      - "6379:6379"
    command: redis-server --requirepass ${REDIS_PASS} --maxmemory 128mb --maxmemory-policy allkeys-lru
    volumes:
      - redis_data:/data
    networks:
      - aipromo-network
    mem_limit: 256m

  qdrant:
    image: qdrant/qdrant:v1.7.4
    container_name: qdrant
    restart: always
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant_data:/qdrant/storage
    networks:
      - aipromo-network
    mem_limit: 512m

volumes:
  rabbitmq_data:
  redis_data:
  qdrant_data:

networks:
  aipromo-network:
    driver: bridge
COMPOSE_EOF

cd /opt/aipromo
docker-compose up -d

# ============================================
# 5. Install Nginx + Certbot
# ============================================

echo "Installing Nginx and Certbot..."
dnf install -y nginx certbot python3-certbot-nginx

# Nginx config — reverse proxy to ECS Backend API
cat > /etc/nginx/conf.d/aipromo.conf << NGINX-EOF
# HTTP server — redirect to HTTPS + Let's Encrypt validation
server {
    listen 80;
    server_name ${DOMAIN_NAME};

    # Let's Encrypt validation location
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    # Redirect all other HTTP traffic to HTTPS
    location / {
        return 301 https://\$host\$request_uri;
    }
}

# HTTPS server — reverse proxy to Backend API
server {
    listen 443 ssl http2;
    server_name ${DOMAIN_NAME};

    ssl_certificate /etc/letsencrypt/live/${DOMAIN_NAME}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${DOMAIN_NAME}/privkey.pem;

    # SSL settings
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;

    # Rate limiting (10 req/s per IP)
    limit_req_zone \$binary_remote_addr zone=api_limit:10m rate=10r/s;

    # CORS headers
    add_header Access-Control-Allow-Origin * always;
    add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
    add_header Access-Control-Allow-Headers "Content-Type, Authorization" always;

    # API routes — reverse proxy to ECS Backend
    location / {
        limit_req zone=api_limit burst=20 nodelay;

        # Handle preflight OPTIONS requests
        if (\$request_method = 'OPTIONS') {
            add_header Access-Control-Allow-Origin * always;
            add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
            add_header Access-Control-Allow-Headers "Content-Type, Authorization" always;
            return 204;
        }

        proxy_pass http://127.0.0.1:${BACKEND_PORT};
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_set_header X-Forwarded-Host \$host;
        proxy_set_header X-Forwarded-Port \$server_port;

        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;

        # Logging
        access_log /var/log/nginx/aipromo-access.log;
        error_log /var/log/nginx/aipromo-error.log;
    }
}
NGINX-EOF

# Create certbot webroot directory
mkdir -p /var/www/certbot

# Start Nginx
systemctl start nginx
systemctl enable nginx

# ============================================
# 6. Request SSL Certificate (Let's Encrypt)
# ============================================

echo "Requesting Let's Encrypt SSL certificate..."
certbot certonly --webroot \
    -w /var/www/certbot \
    -d ${DOMAIN_NAME} \
    --non-interactive \
    --agree-tos \
    --email admin@${DOMAIN_NAME} \
    --keep-until-expiring \
    || echo "Certbot failed — SSL cert needs manual setup. Run: certbot certonly --webroot -w /var/www/certbot -d ${DOMAIN_NAME}"

# Setup auto-renewal cron job
(crontab -l 2>/dev/null; echo "0 3 * * 1 certbot renew --quiet && systemctl reload nginx") | crontab -

# ============================================
# 7. Setup Log Rotation
# ============================================

echo "Setting up log rotation..."
cat > /etc/logrotate.d/aipromo << 'LOGROTATE-EOF'
/var/log/nginx/aipromo-*.log {
    daily
    missingok
    rotate 7
    compress
    delaycompress
    notifempty
    create 0640 nginx nginx
    sharedscripts
    postrotate
        systemctl reload nginx
    endscript
}
LOGROTATE-EOF

# ============================================
# 8. Docker Cleanup Cron (weekly)
# ============================================

echo "Setting up Docker cleanup cron..."
(crontab -l 2>/dev/null; echo "0 4 * * 0 docker system prune -f --filter 'until=168h'") | crontab -

# ============================================
# 9. Final Setup
# ============================================

echo "EC2 initialization complete!"
echo "   - Docker + ECS Agent: Running"
echo "   - Redis: Port 6379 (internal)"
echo "   - RabbitMQ: Port 5672 (internal)"
echo "   - Qdrant: Port 6333 (internal)"
echo "   - Nginx: Port 80/443 (public)"
echo "   - Swap: 2GB"
echo "   - Domain: ${DOMAIN_NAME}"
echo ""
echo "   Backend API will be available at: https://${DOMAIN_NAME}"
echo "   After ECS deploys the backend-api task."
