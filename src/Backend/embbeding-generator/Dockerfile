# ─────────────────────────────────────────────────────────────
# Embedding Generator - Flexible CPU/GPU Dockerfile
# ─────────────────────────────────────────────────────────────

FROM python:3.11-slim-bookworm

# Basic Python & HuggingFace optimizations
# HF_HOME ensures sentence-transformers cache doesn't write to root
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    HF_HOME=/app/cache

WORKDIR /app

# Install minimal runtime dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    tini \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy requirements first (for Docker layer caching)
COPY requirements.txt .

# -------------------------------------------------------------
# PYTORCH INSTALLATION (Flexible Layer)
# Default is CPU to keep the image small (~1GB). 
# To build for GPU, run: docker build --build-arg TORCH_INDEX_URL="https://pypi.org/simple" .
# -------------------------------------------------------------
ARG TORCH_INDEX_URL="https://download.pytorch.org/whl/cpu"
RUN pip install --no-cache-dir torch==2.2.2 --index-url ${TORCH_INDEX_URL}

# Install remaining dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy application code and scripts
COPY src/ ./src/
COPY scripts/ ./scripts/

# Prepare model and cache directories, then assign permissions BEFORE switching user
RUN mkdir -p /app/models /app/cache && \
    chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Download model at build time (Runs as appuser to ensure proper permissions)
RUN python scripts/download_model.py

# Environment variables
ENV PYTHONPATH=/app \
    MODEL_PATH=/app/models/bge-small-en-v1.5 \
    HTTP_HOST=0.0.0.0 \
    HTTP_PORT=8001 \
    RABBITMQ_HOST=rabbitmq \
    RABBITMQ_PORT=5672 \
    RABBITMQ_USER=admin \
    RABBITMQ_PASS=admin \
    REQUEST_QUEUE=embedding.requests \
    RESPONSE_QUEUE=embedding.responses \
    ENABLE_HTTP_API=true \
    LOG_LEVEL=INFO

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=120s --retries=3 \
  CMD curl -f http://localhost:8001/health || exit 1

EXPOSE 8001

ENTRYPOINT ["/usr/bin/tini", "--"]

# Start app
CMD ["python", "-m", "src.main"]