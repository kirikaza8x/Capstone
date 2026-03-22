# ─────────────────────────────────────────────────────────────
# Embedding Generator — ONNX + Multi-stage (lightweight)
# Stage 1: Builder  — downloads, exports ONNX, installs deps
# Stage 2: Runtime  — only what's needed to serve
# Target image size: ~280MB
# ─────────────────────────────────────────────────────────────

# ── Stage 1: Builder ─────────────────────────────────────────
FROM python:3.11.12-slim-bookworm AS builder

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    HF_HOME=/tmp/hf_cache \
    PIP_NO_CACHE_DIR=1

WORKDIR /build

RUN apt-get update && apt-get upgrade -y && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .

# Install to system (not --prefix) so scripts can find packages directly
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install torch==2.2.2+cpu \
        --index-url https://download.pytorch.org/whl/cpu && \
    pip install -r requirements.txt

# Copy source + scripts
COPY src/ ./src/
COPY scripts/ ./scripts/

# PYTHONPATH=/build so src.* imports work inside download_model.py
RUN --mount=type=cache,target=/tmp/hf_cache \
    PYTHONPATH=/build \
    MODEL_PATH=/build/models/bge-small-en-v1.5 \
    python scripts/download_model.py

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM python:3.11.12-slim-bookworm AS runtime

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PYTHONPATH=/app \
    HOME=/app \
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

WORKDIR /app

RUN apt-get update && apt-get upgrade -y && apt-get install -y --no-install-recommends \
    curl tini \
    && rm -rf /var/lib/apt/lists/*

# Non-root user
RUN groupadd -r appuser && \
    useradd -r -g appuser -d /app -s /sbin/nologin appuser

# Copy installed packages from builder
# Installs to system python so no --prefix path issues
COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin

# Copy source code
COPY --from=builder /build/src ./src/

# Copy ONNX model only (~140MB)
COPY --from=builder /build/models ./models/

RUN mkdir -p /app/models /app/cache && \
    chown -R appuser:appuser /app

USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8001/health || exit 1

EXPOSE 8001
ENTRYPOINT ["/usr/bin/tini", "--"]
CMD ["python", "-m", "src.main"]