#!/bin/bash
# ============================================
# EC2 User Data — Amazon Linux 2 ECS-Optimized AMI
# ============================================

# DO NOT use set -e or set -u - they cause silent failures in user_data
set -o pipefail

# Redirect all output to a log file for debugging
exec > >(tee /var/log/user-data.log) 2>&1

echo "============================================"
echo " EC2 User Data Script Starting"
echo "============================================"

# ============================================
# 1. Create 2GB Swap File
# ============================================

echo "Creating 2GB swap file..."

if [ ! -f /swapfile ]; then
    dd if=/dev/zero of=/swapfile bs=128M count=16
    chmod 600 /swapfile
    mkswap /swapfile
    swapon /swapfile
    echo "/swapfile none swap sw 0 0" >> /etc/fstab
    echo "Swap file created and activated."
else
    echo "Swap file already exists, skipping."
fi

# ============================================
# 2. Configure ECS Agent
# ============================================

echo "Configuring ECS Agent..."
mkdir -p /etc/ecs

cat > /etc/ecs/ecs.config << EOF
ECS_CLUSTER=${project}-${environment}-cluster
ECS_ENGINE_AUTH_TYPE=dockercfg
ECS_AVAILABLE_LOGGING_DRIVERS=["json-file","awslogs"]
ECS_LOGLEVEL=info
ECS_CONTAINER_INSTANCE_TAGS={"Project": "${project}", "Environment": "${environment}"}
EOF

echo "ECS config written to /etc/ecs/ecs.config"

# ============================================
# 3. Install Docker Compose
# ============================================

echo "Installing Docker Compose..."

if [ ! -f /usr/local/bin/docker-compose ]; then
    curl -L "https://github.com/docker/compose/releases/download/v2.24.0/docker-compose-Linux-x86_64" \
         -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    echo "Docker Compose installed: $(docker-compose --version)"
else
    echo "Docker Compose already installed."
fi

# ============================================
# 4. Start Infrastructure Containers
# ============================================

echo "Creating infrastructure docker-compose configuration..."
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
    environment:
      RABBITMQ_DEFAULT_USER: ${rabbitmq_user}
      RABBITMQ_DEFAULT_PASS: ${rabbitmq_pass}
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
    command: >
      redis-server
      --requirepass ${redis_pass}
      --maxmemory 128mb
      --maxmemory-policy allkeys-lru
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

# Wait for Docker daemon to be ready
echo "Waiting for Docker daemon..."
for i in $(seq 1 20); do
    if docker info >/dev/null 2>&1; then
        echo "Docker daemon is ready!"
        break
    fi
    echo "  Waiting for Docker... ($i/20)"
    sleep 3
done

cd /opt/aipromo
docker-compose up -d
echo "Infrastructure containers started."

# ============================================
# 5. Start ECS Agent
# ============================================

echo "Starting ECS Agent..."

# Enable ECS Agent
systemctl enable ecs

# Start ECS Agent in background with timeout to prevent hanging
timeout 60 systemctl start ecs || {
    echo "WARNING: ECS Agent start timed out or failed"
    # Try to start it one more time in background
    nohup systemctl start ecs &>/dev/null &
}

echo "ECS Agent start initiated."

# ============================================
# 6. Install Nginx + Certbot and Set Up HTTPS
# ============================================

echo "Installing Nginx..."
amazon-linux-extras install nginx1.12 -y || true

echo "Installing Certbot for SSL..."
amazon-linux-extras install epel -y || true

# Amazon Linux 2 ships Python 2.7, so use python2-certbot-nginx.
yum install -y certbot python2-certbot-nginx || true

mkdir -p /var/www/certbot

# ── Fix Nginx Configuration Conflict ──────────────────────────────────────
# Amazon Linux 2 default nginx.conf contains a hardcoded server block that
# conflicts with our custom config in conf.d. We overwrite it with a clean
# version that only includes conf.d/*.conf to avoid "duplicate server" errors.

cat > /etc/nginx/nginx.conf << 'NGINX_CORE_EOF'
user nginx;
worker_processes 1;
error_log /var/log/nginx/error.log;
pid /run/nginx.pid;

include /usr/share/nginx/modules/*.conf;

events {
    worker_connections 1024;
}

http {
    log_format  main  '$remote_addr - $remote_user [$time_local] "$request" '
                      '$status $body_bytes_sent "$http_referer" '
                      '"$http_user_agent" "$http_x_forwarded_for"';

    access_log  /var/log/nginx/access.log  main;

    sendfile            on;
    tcp_nopush          on;
    tcp_nodelay         on;
    keepalive_timeout   65;
    types_hash_max_size 2048;

    include             /etc/nginx/mime.types;
    default_type        application/octet-stream;

    # Load custom configs from conf.d
    include /etc/nginx/conf.d/*.conf;
}
NGINX_CORE_EOF

# Remove default config file just in case
rm -f /etc/nginx/conf.d/default.conf

echo "Nginx core configuration fixed."

# ── Rate-limit zone ───────────────────────────────────────────────────────
cat > /etc/nginx/conf.d/rate-limit.conf << 'EOF'
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
EOF

# ── Step A: Write Initial Nginx Config (HTTP Fallback) ────────────────────
# We start with HTTP that allows API traffic + Certbot challenges.
# If SSL cert is obtained later, we upgrade to HTTPS.

cat > /etc/nginx/conf.d/aipromo.conf << NGINX_HTTP_EOF
server {
    listen 80 default_server;
    server_name ${domain_name};

    # Allow Certbot challenges even on HTTP fallback
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    add_header Access-Control-Allow-Origin  * always;
    add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
    add_header Access-Control-Allow-Headers "Content-Type, Authorization" always;

    location / {
        limit_req zone=api_limit burst=20 nodelay;

        if (\$request_method = 'OPTIONS') { return 204; }

        proxy_pass http://127.0.0.1:${backend_port};

        proxy_set_header Host              \$host;
        proxy_set_header X-Real-IP         \$remote_addr;
        proxy_set_header X-Forwarded-For   \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
NGINX_HTTP_EOF

# Start Nginx immediately so API is available while waiting for SSL
nginx -t && systemctl start nginx && systemctl enable nginx
echo "Nginx started with HTTP fallback config."

# ── Step B: Request SSL Certificate ───────────────────────────────────────
echo "Attempting to obtain Let's Encrypt SSL certificate..."

MAX_RETRIES=5
RETRY_INTERVAL=30
SSL_SUCCESS=false

for i in $(seq 1 $${MAX_RETRIES}); do
    echo "--- Certbot attempt $$i / $${MAX_RETRIES} ---"

    # Check DNS resolution BEFORE calling Certbot
    if ! nslookup ${domain_name} > /dev/null 2>&1; then
        echo "DNS not yet resolvable for ${domain_name}. Waiting $${RETRY_INTERVAL}s before retry..."
        sleep $${RETRY_INTERVAL}
        continue
    fi

    echo "DNS resolved. Requesting certificate..."

    if certbot certonly --webroot \
        -w /var/www/certbot \
        -d ${domain_name} \
        --non-interactive \
        --agree-tos \
        --email admin@${domain_name} \
        --keep-until-expiring; then

        echo "SSL certificate obtained successfully on attempt $$i!"
        SSL_SUCCESS=true
        break
    else
        echo "Certbot failed on attempt $$i. Waiting $${RETRY_INTERVAL}s before retry..."
        sleep $${RETRY_INTERVAL}
    fi
done

# ── Step C: Upgrade to HTTPS if successful ────────────────────────────────

if [ "$${SSL_SUCCESS}" = "true" ] || [ -f "/etc/letsencrypt/live/${domain_name}/fullchain.pem" ]; then
    echo "SSL cert found. Upgrading to HTTPS Nginx config..."

    cat > /etc/nginx/conf.d/aipromo.conf << NGINX_SSL_EOF
server {
    listen 80;
    server_name ${domain_name};

    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }

    location / {
        return 301 https://\$host\$request_uri;
    }
}

server {
    listen 443 ssl http2;
    server_name ${domain_name};

    ssl_certificate     /etc/letsencrypt/live/${domain_name}/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/${domain_name}/privkey.pem;

    ssl_protocols TLSv1.2;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-CHACHA20-POLY1305:ECDHE-ECDSA-AES128-SHA256:ECDHE-RSA-AES128-SHA256:ECDHE-ECDSA-AES128-SHA:ECDHE-RSA-AES256-SHA384:ECDHE-RSA-AES128-SHA:ECDHE-ECDSA-AES256-SHA384:ECDHE-ECDSA-AES256-SHA:ECDHE-RSA-AES256-SHA:DHE-RSA-AES128-SHA256:DHE-RSA-AES128-SHA:DHE-RSA-AES256-SHA256:DHE-RSA-AES256-SHA:AES128-GCM-SHA256:AES256-GCM-SHA384:AES128-SHA256:AES256-SHA256:AES128-SHA:AES256-SHA:!DSS';

    add_header Access-Control-Allow-Origin  * always;
    add_header Access-Control-Allow-Methods "GET, POST, PUT, DELETE, OPTIONS" always;
    add_header Access-Control-Allow-Headers "Content-Type, Authorization" always;

    location / {
        limit_req zone=api_limit burst=20 nodelay;

        if (\$request_method = 'OPTIONS') { return 204; }

        proxy_pass http://127.0.0.1:${backend_port};

        proxy_set_header Host              \$host;
        proxy_set_header X-Real-IP         \$remote_addr;
        proxy_set_header X-Forwarded-For   \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
NGINX_SSL_EOF

    nginx -t && systemctl reload nginx
    echo "Nginx upgraded to HTTPS."
else
    echo "WARNING: Could not obtain SSL certificate. Keeping HTTP fallback mode."
    echo "         To enable HTTPS later, run certbot manually and update Nginx config."
fi

# ── Step D: Schedule Automatic Certificate Renewal ───────────────────────
(crontab -l 2>/dev/null; echo "0 3 * * 1 certbot renew --quiet && systemctl reload nginx") | crontab -
echo "Cert auto-renewal cron job registered."

echo "============================================"
echo " EC2 initialization complete!"
echo "============================================"
