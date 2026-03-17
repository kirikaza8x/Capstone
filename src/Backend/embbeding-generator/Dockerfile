
FROM baseImage
# FROM python:3.11-slim-bookworm

# # Security: Create non-root user
# RUN groupadd -r appuser && useradd -r -g appuser appuser

# WORKDIR /app

# # Install runtime dependencies and patch system
# RUN apt-get update && apt-get upgrade -y \
#     && apt-get install -y --no-install-recommends curl git tini \
#     && rm -rf /var/lib/apt/lists/* \
#     && apt-get clean

# # Copy requirements and install
# COPY requirements.txt .
# RUN pip install --no-cache-dir -r requirements.txt

# # Copy application code
# COPY src/ ./src/
# COPY scripts/ ./scripts/

# # Create models directory with correct permissions
# RUN mkdir -p /app/models && chown -R appuser:appuser /app

# # Switch to non-root user
# USER appuser

# # Environment variables
# ENV PYTHONPATH=/app \
#     PYTHONUNBUFFERED=1 \
#     PYTHONDONTWRITEBYTECODE=1 \
#     MODEL_PATH=/app/models/bge-small-en-v1.5 \
#     HTTP_HOST=0.0.0.0 \
#     HTTP_PORT=8001 \
#     RABBITMQ_HOST=rabbitmq \
#     RABBITMQ_PORT=5672 \
#     RABBITMQ_USER=admin \
#     RABBITMQ_PASS=admin \
#     REQUEST_QUEUE=embedding.requests \
#     RESPONSE_QUEUE=embedding.responses \
#     ENABLE_HTTP_API=true \
#     LOG_LEVEL=INFO

# # Health check
# HEALTHCHECK --interval=30s --timeout=10s --start-period=120s --retries=3 \
#   CMD curl -f http://localhost:8001/health || exit 1

# EXPOSE 8001

# ENTRYPOINT ["/usr/bin/tini", "--"]

# CMD ["sh", "-c", "python scripts/download_model.py && python -m src.main"]
