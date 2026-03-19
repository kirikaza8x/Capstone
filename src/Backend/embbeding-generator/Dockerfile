# ─────────────────────────────────────────────────────────────
# Embedding Generator - Optimized CPU-only Dockerfile
# ─────────────────────────────────────────────────────────────

FROM python:3.11-slim-bookworm

# Basic Python optimizations
ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1

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

#  Install torch FIRST (separate layer for caching)
RUN pip install --no-cache-dir torch==2.2.2

# Install remaining dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy application code
COPY src/ ./src/
COPY scripts/ ./scripts/

# Prepare model directory
RUN mkdir -p /app/models && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

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

#  Download model at build time (no delay at runtime)
RUN python scripts/download_model.py

# Start app
CMD ["python", "-m", "src.main"]