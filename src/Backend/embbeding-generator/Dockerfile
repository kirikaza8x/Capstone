# ─────────────────────────────────────────────────────────────
# Embedding Generator — ONNX + Multi-stage (lightweight)
# Stage 1: Builder  — downloads, exports ONNX, installs deps
# Stage 2: Runtime  — only what's needed to serve
# Target image size: ~280MB
# ─────────────────────────────────────────────────────────────

# ── Stage 1: Builder ─────────────────────────────────────────
FROM python:3.11-slim-bookworm AS builder

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    HF_HOME=/tmp/hf_cache \
    PIP_NO_CACHE_DIR=1

WORKDIR /build

# Install build deps
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Install ALL deps including torch (needed only for ONNX export)
COPY requirements.txt .

# torch needed at build time for ONNX export — NOT copied to runtime stage
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install --prefix=/install torch==2.2.2 \
        --index-url https://download.pytorch.org/whl/cpu && \
    pip install --prefix=/install -r requirements.txt

# Copy source + scripts
COPY src/ ./src/
COPY scripts/ ./scripts/

# Download snapshot + export to ONNX
# torch is available here for the export step
RUN --mount=type=cache,target=/tmp/hf_cache \
    PYTHONPATH=/build \
    MODEL_PATH=/build/models/bge-small-en-v1.5 \
    python scripts/download_model.py

# ── Stage 2: Runtime ─────────────────────────────────────────
FROM python:3.11-slim-bookworm AS runtime

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

RUN apt-get update && apt-get install -y --no-install-recommends \
    curl tini \
    && rm -rf /var/lib/apt/lists/*

# Non-root user
RUN groupadd -r appuser && \
    useradd -r -g appuser -d /app -s /sbin/nologin appuser

# Copy ONLY installed packages from builder (no torch, no build cache)
COPY --from=builder /install /usr/local

# Copy source code
COPY --from=builder /build/src ./src/

# Copy ONNX model (no safetensors, no pytorch weights)
COPY --from=builder /build/models ./models/

RUN mkdir -p /app/models /app/cache && \
    chown -R appuser:appuser /app

USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8001/health || exit 1

EXPOSE 8001
ENTRYPOINT ["/usr/bin/tini", "--"]
CMD ["python", "-m", "src.main"]