"""
Configuration management using environment variables.
"""

import os
from pathlib import Path
from dotenv import load_dotenv

# Load .env file if exists
load_dotenv()

# ─────────────────────────────────────────────────────────────
# RabbitMQ Configuration
# ─────────────────────────────────────────────────────────────

RABBITMQ_HOST = os.getenv("RABBITMQ_HOST", "localhost")
RABBITMQ_PORT = int(os.getenv("RABBITMQ_PORT", "5672"))
RABBITMQ_USER = os.getenv("RABBITMQ_USER", "admin")
RABBITMQ_PASS = os.getenv("RABBITMQ_PASS", "admin")
REQUEST_QUEUE = os.getenv("REQUEST_QUEUE", "embedding.requests")
RESPONSE_QUEUE = os.getenv("RESPONSE_QUEUE", "embedding.responses")

# ─────────────────────────────────────────────────────────────
# HTTP API Configuration
# ─────────────────────────────────────────────────────────────

ENABLE_HTTP_API = os.getenv("ENABLE_HTTP_API", "true").lower() == "true"
HTTP_HOST = os.getenv("HTTP_HOST", "localhost")
HTTP_PORT = int(os.getenv("HTTP_PORT", "8001"))

# ─────────────────────────────────────────────────────────────
# Model Configuration
# ─────────────────────────────────────────────────────────────

MODEL_NAME = os.getenv("MODEL_NAME", "bge-small-en-v1.5")
MODEL_PATH = Path(os.getenv("MODEL_PATH", "models/bge-small-en-v1.5"))

# ─────────────────────────────────────────────────────────────
# Processing Configuration
# ─────────────────────────────────────────────────────────────

BATCH_SIZE = int(os.getenv("BATCH_SIZE", "32"))
NORMALIZE_EMBEDDINGS = os.getenv("NORMALIZE_EMBEDDINGS", "true").lower() == "true"

# ─────────────────────────────────────────────────────────────
# Retry Configuration
# ─────────────────────────────────────────────────────────────

MAX_RETRIES = int(os.getenv("MAX_RETRIES", "10"))
RETRY_DELAY = float(os.getenv("RETRY_DELAY", "2.0"))
RECONNECT_INTERVAL = int(os.getenv("RECONNECT_INTERVAL", "30"))

# ─────────────────────────────────────────────────────────────
# Application Configuration
# ─────────────────────────────────────────────────────────────

BASE_DIR = Path(__file__).parent.parent
LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")

# ─────────────────────────────────────────────────────────────
# Helper: Detect environment
# ─────────────────────────────────────────────────────────────

def is_docker_environment() -> bool:
    """Detect if running inside Docker."""
    return os.path.exists('/.dockerenv') or os.getenv('KUBERNETES_SERVICE_HOST') is not None