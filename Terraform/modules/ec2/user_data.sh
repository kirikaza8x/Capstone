#!/bin/bash
# ============================================
# EC2 User Data — Amazon Linux 2 ECS-Optimized AMI
# Fully Automated Infrastructure Setup
# ============================================

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

# Properly formatted ECS config with valid JSON
cat > /etc/ecs/ecs.config << EOF
ECS_CLUSTER=${project}-${environment}-cluster
ECS_ENABLE_TASK_IAM_ROLE=true
ECS_ENGINE_AUTH_TYPE=dockercfg
ECS_AVAILABLE_LOGGING_DRIVERS=["json-file","awslogs"]
ECS_LOGLEVEL=info
ECS_CONTAINER_INSTANCE_TAGS={"Project": "${project}", "Environment": "${environment}"}
EOF

echo "ECS config written to /etc/ecs/ecs.config"
cat /etc/ecs/ecs.config

# ============================================
# 3. Install Docker Compose & Dependencies
# ============================================

echo "Installing dependencies..."

# DNS tools required for Auto-HTTPS script checks
yum install -y bind-utils || true
amazon-linux-extras install epel -y || true
amazon-linux-extras install nginx1.12 -y || true
yum install -y certbot python2-certbot-nginx || true

# Docker Compose
if [ ! -f /usr/local/bin/docker-compose ]; then
    curl -L "https://github.com/docker/compose/releases/download/v2.24.0/docker-compose-Linux-x86_64" \
         -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    echo "Docker Compose installed."
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
    image: qdrant/qdrant:latest
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

  n8n:
    image: n8nio/n8n:latest
    container_name: n8n
    restart: always
    ports:
      - "5678:5678"
    environment:
      DB_TYPE: postgresdb
      DB_POSTGRESDB_HOST: ${db_host}
      DB_POSTGRESDB_PORT: 5432
      DB_POSTGRESDB_DATABASE: ${db_name}
      DB_POSTGRESDB_USER: ${db_username}
      DB_POSTGRESDB_PASSWORD: ${db_password}
      N8N_REDIS_HOST: redis
      N8N_REDIS_PORT: 6379
      N8N_REDIS_PASSWORD: ${redis_pass}
      N8N_PORT: 5678
      N8N_HOST: 0.0.0.0
      N8N_PROTOCOL: http
      N8N_ENCRYPTION_KEY: ${n8n_encryption_key}
    volumes:
      - n8n_data:/home/node/.n8n
    networks:
      - aipromo-network
    mem_limit: 512m
    depends_on:
      - redis

volumes:
  rabbitmq_data:
  redis_data:
  qdrant_data:
  n8n_data:
  
networks:
  aipromo-network:
    driver: bridge
COMPOSE_EOF

# Wait for Docker daemon
echo "Waiting for Docker daemon..."
for i in $(seq 1 20); do
    if docker info >/dev/null 2>&1; then
        echo "Docker daemon is ready!"
        break
    fi
    echo "  Waiting for Docker... ($i/20)"
    sleep 3
done

# Start infrastructure containers using explicit path (avoid cd issues)
(cd /opt/aipromo && docker-compose up -d)
echo "Infrastructure containers started."

# ============================================
# 5. Start ECS Agent
# ============================================

echo "=========================================="
echo " Starting ECS Agent"
echo "=========================================="

# Pre-pull ECS agent image to avoid hanging during systemctl start
echo "Pre-pulling ECS agent image (this may take 2-3 minutes)..."
docker pull amazon/amazon-ecs-agent:latest || {
    echo "WARNING: Failed to pre-pull ECS agent image. Will retry during service start."
}

# Ensure Docker daemon is fully running before starting ECS agent
echo "Verifying Docker daemon status..."
for i in $(seq 1 30); do
    if docker info >/dev/null 2>&1; then
        echo "Docker daemon is running and healthy!"
        break
    fi
    echo "  Waiting for Docker daemon... ($i/30)"
    sleep 2
done

# Verify Docker is working
if ! docker info >/dev/null 2>&1; then
    echo "ERROR: Docker daemon is not running after 60 seconds. ECS agent will fail."
    echo "Check: systemctl status docker"
else
    echo "Starting ECS agent service..."

    # Stop and restart to ensure clean state
    systemctl stop ecs || true
    sleep 2

    systemctl enable ecs

    # Start ECS agent with timeout to prevent hanging
    timeout 120 systemctl start ecs || {
        echo "WARNING: systemctl start ecs timed out. Checking status..."
        systemctl status ecs || true
        # Try starting directly with ecs-init as fallback
        nohup /usr/libexec/amazon-ecs-init start &>/dev/null &
    }

    echo "ECS agent start command executed. Waiting for agent to register..."

    # Wait for ECS agent to register with the cluster
    echo "Waiting for ECS agent to connect to cluster (${project}-${environment}-cluster)..."
    for i in $(seq 1 60); do
        if docker ps --filter "name=ecs-agent" --filter "status=running" | grep -q ecs-agent; then
            # Check if agent has registered
            if cat /var/log/ecs/ecs-agent.log 2>/dev/null | grep -q "Registered!"; then
                echo "ECS agent successfully registered with cluster!"
                break
            fi
        fi
        echo "  Waiting for ECS agent registration... ($i/60)"
        sleep 5
    done

    # Final verification
    if docker ps --filter "name=ecs-agent" --filter "status=running" | grep -q ecs-agent; then
        echo "ECS agent container is running"
    else
        echo "WARNING: ECS agent container is NOT running"
        echo "Check logs: cat /var/log/ecs/ecs-agent.log"
        echo "Manual fix: docker pull amazon/amazon-ecs-agent:latest && systemctl restart ecs"
    fi
fi

# ============================================
# 6. Setup Nginx & Auto-HTTPS Mechanism
# ============================================

# Fix Nginx Core to avoid "duplicate server" error
cat > /etc/nginx/nginx.conf << 'NGINX_CORE_EOF'
user nginx;
worker_processes 1;
error_log /var/log/nginx/error.log;
pid /run/nginx.pid;

include /usr/share/nginx/modules/*.conf;

events { worker_connections 1024; }

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
    include /etc/nginx/conf.d/*.conf;
}
NGINX_CORE_EOF

rm -f /etc/nginx/conf.d/default.conf
mkdir -p /var/www/certbot

# ── Step A: Start Nginx in HTTP Mode immediately ──────────────────────────
cat > /etc/nginx/conf.d/aipromo.conf << NGINX_HTTP_EOF
server {
    listen 80 default_server;
    server_name ${domain_name};
    location /.well-known/acme-challenge/ { root /var/www/certbot; }
    location / {
        proxy_pass http://127.0.0.1:${backend_port};
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
    }
}
NGINX_HTTP_EOF

nginx -t && systemctl start nginx && systemctl enable nginx
echo "Nginx started with HTTP fallback."

# ── Step B: Create Auto-HTTPS Script ─────────────────────────────────────
cat > /usr/local/bin/auto-setup-https.sh << 'AUTO_SSL_SCRIPT'
#!/bin/bash
DOMAIN="${domain_name}"
CERT_PATH="/etc/letsencrypt/live/${domain_name}/fullchain.pem"
LOG="/var/log/https-setup.log"

# Function to enable HTTPS in Nginx
enable_nginx_https() {
    cat > /etc/nginx/conf.d/aipromo.conf << NGINX_SSL_CONF_EOF
server {
    listen 80;
    server_name $DOMAIN;
    location /.well-known/acme-challenge/ { root /var/www/certbot; }
    location / { return 301 https://\$host\$request_uri; }
}

server {
    listen 443 ssl http2;
    server_name $DOMAIN;

    ssl_certificate     /etc/letsencrypt/live/$DOMAIN/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/$DOMAIN/privkey.pem;

    ssl_protocols TLSv1.2;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384:ECDHE-RSA-AES256-GCM-SHA384:DHE-RSA-AES128-GCM-SHA256:DHE-RSA-AES256-GCM-SHA384:ECDHE-ECDSA-CHACHA20-POLY1305:ECDHE-RSA-CHACHA20-POLY1305:DHE-RSA-CHACHA20-POLY1305:ECDHE-ECDSA-AES128-SHA256:ECDHE-RSA-AES128-SHA256:ECDHE-ECDSA-AES128-SHA:ECDHE-RSA-AES256-SHA384:ECDHE-RSA-AES128-SHA:ECDHE-ECDSA-AES256-SHA384:ECDHE-ECDSA-AES256-SHA:ECDHE-RSA-AES256-SHA:DHE-RSA-AES128-SHA256:DHE-RSA-AES128-SHA:DHE-RSA-AES256-SHA256:DHE-RSA-AES256-SHA:AES128-GCM-SHA256:AES256-GCM-SHA384:AES128-SHA256:AES256-SHA256:AES128-SHA:AES256-SHA:!DSS';

    location / {
        proxy_pass http://127.0.0.1:${backend_port};
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
NGINX_SSL_CONF_EOF

    nginx -t && systemctl reload nginx
}

# Check if already done
if [ -f "$CERT_PATH" ]; then
    echo "$(date): Certificate found. Ensuring Nginx uses HTTPS..." >> $LOG
    # Verify if Nginx is actually using HTTPS
    if ! grep -q "ssl_certificate" /etc/nginx/conf.d/aipromo.conf; then
        enable_nginx_https
        echo "$(date): Nginx upgraded to HTTPS successfully." >> $LOG
    else
        echo "$(date): HTTPS is already active." >> $LOG
    fi
    # Stop retrying
    (crontab -l 2>/dev/null | grep -v "auto-setup-https.sh") | crontab -
    exit 0
fi

# Check DNS
if ! nslookup $DOMAIN > /dev/null 2>&1; then
    echo "$(date): DNS not yet resolved for $DOMAIN. Waiting..." >> $LOG
    exit 1
fi

echo "$(date): DNS resolved. Attempting to obtain SSL..." >> $LOG

if certbot certonly --webroot \
    -w /var/www/certbot \
    -d $DOMAIN \
    --non-interactive \
    --agree-tos \
    --email admin@$DOMAIN \
    --keep-until-expiring; then
    
    echo "$(date): SUCCESS! Obtained SSL certificate." >> $LOG
    enable_nginx_https
    echo "$(date): HTTPS activated. Stopping script." >> $LOG
    (crontab -l 2>/dev/null | grep -v "auto-setup-https.sh") | crontab -
else
    echo "$(date): Failed to obtain certificate. Will retry in 5 mins." >> $LOG
    exit 1
fi
AUTO_SSL_SCRIPT

chmod +x /usr/local/bin/auto-setup-https.sh

# ── Step C: Schedule & Run ───────────────────────────────────────────────
/usr/local/bin/auto-setup-https.sh
(crontab -l 2>/dev/null; echo "*/5 * * * * /usr/local/bin/auto-setup-https.sh") | crontab -

# ============================================
# 7. Setup Periodic Docker Cleanup
# ============================================

echo "[7/7] Setting up periodic Docker cleanup..."

# Create cleanup script
CLEANUP_SCRIPT="/usr/local/bin/docker-cleanup.sh"

cat > "$CLEANUP_SCRIPT" << 'EOF'
#!/bin/bash
# Docker cleanup script — runs daily to prevent disk full
echo "[$(date)] Starting Docker cleanup..." >> /var/log/docker-cleanup.log

# Remove stopped containers
docker container prune -f >> /var/log/docker-cleanup.log 2>&1

# Remove unused images (keep only those used by running containers, older than 24h)
docker image prune -af --filter "until=24h" >> /var/log/docker-cleanup.log 2>&1

# Remove unused volumes
docker volume prune -f >> /var/log/docker-cleanup.log 2>&1

# Remove unused networks
docker network prune -f >> /var/log/docker-cleanup.log 2>&1

# Log disk usage after cleanup
df -h / >> /var/log/docker-cleanup.log 2>&1

echo "[$(date)] Cleanup complete." >> /var/log/docker-cleanup.log
EOF

chmod +x "$CLEANUP_SCRIPT"

# Schedule cleanup to run daily at 3 AM
if crontab -l 2>/dev/null | grep -q "docker-cleanup.sh"; then
    echo "Docker cleanup cron job already exists. Skipping."
else
    echo "Scheduling daily Docker cleanup at 3 AM..."
    (crontab -l 2>/dev/null; echo "0 3 * * * $CLEANUP_SCRIPT") | crontab -
    echo "Docker cleanup scheduled."
fi

# ============================================
# 8. Initial Cleanup — Ensure enough resources for ECS tasks
# ============================================

echo "[8/8] Running initial Docker cleanup to free resources..."

# Remove stopped containers
docker container prune -f || true

# Remove unused images (older than 24h)
docker image prune -af --filter "until=24h" || true

# Remove unused volumes
docker volume prune -f || true

# Remove unused networks
docker network prune -f || true

echo "Initial cleanup complete."
echo "============================================"
echo " EC2 initialization complete!"
echo " Server will auto-upgrade to HTTPS when DNS is ready."
echo " Docker cleanup scheduled daily at 3 AM."
echo "============================================"
