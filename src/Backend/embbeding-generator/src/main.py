"""
Embedding Service - Entry Point

Runs BOTH:
1. RabbitMQ consumer (primary production interface) with retry logic
2. HTTP API on configurable port (for testing/debugging with Swagger)

Run: python -m src.main
"""

import asyncio
import os
import sys
from pathlib import Path

project_root = Path(__file__).parent.parent
sys.path.insert(0, str(project_root))

from src.config import (
    ENABLE_HTTP_API, HTTP_HOST, HTTP_PORT,
    RABBITMQ_HOST, MODEL_PATH,
    REQUEST_QUEUE, RESPONSE_QUEUE,
)
from src.utils.logging import setup_logging
from src.embedding.loader import ModelLoader
from src.embedding.service import EmbeddingService
from src.messaging.consumer import RabbitMQConsumer
from src.http.app import create_app

logger = setup_logging("main")


async def run_http_server(app):
    """Run FastAPI HTTP server."""
    import uvicorn

    config = uvicorn.Config(
        app=app,
        host=HTTP_HOST,
        port=HTTP_PORT,
        log_level="info",
        access_log=False,
    )
    server = uvicorn.Server(config)
    await server.serve()


async def main():
    """Main entry point — runs RabbitMQ consumer + optional HTTP server."""
    model_name = Path(MODEL_PATH).name
    device     = os.getenv("MODEL_DEVICE", "cpu")

    logger.info("=" * 70)
    logger.info("🚀 Starting Embedding Service")
    logger.info(f"📦 Model: {model_name} on {device}")
    logger.info(f"🔌 RabbitMQ: {'Enabled' if RABBITMQ_HOST else 'Disabled'} (Production interface)")
    logger.info(f"🌐 HTTP: {'Enabled' if ENABLE_HTTP_API else 'Disabled'} (Swagger testing)")
    logger.info("=" * 70)

    try:
        # ── Step 1: Load ONNX model ───────────────────────────────────
        logger.info("📦 Step 1: Loading model...")
        loader = ModelLoader(Path(MODEL_PATH))
        model, tokenizer = loader.load(device=device)   # unpack tuple

        # ── Step 2: Init embedding service ───────────────────────────
        logger.info("🔧 Step 2: Initializing embedding service...")
        embedding_service = EmbeddingService(
            model=model,
            tokenizer=tokenizer,
            dimension=loader.get_dimension(),
            model_name=model_name,
        )

        # ── Step 3: Start RabbitMQ consumer ──────────────────────────
        consumer_task = None
        if RABBITMQ_HOST:
            logger.info("🔌 Step 3: Connecting to RabbitMQ...")
            consumer      = RabbitMQConsumer(embedding_service)
            consumer_task = asyncio.create_task(consumer.start())
            logger.info("🔌 RabbitMQ consumer started (will retry if disconnected)")
        else:
            logger.info("⚠️  RabbitMQ disabled (RABBITMQ_HOST not set)")

        # ── Step 4: Start HTTP server ─────────────────────────────────
        http_task = None
        if ENABLE_HTTP_API:
            logger.info("🌐 Step 4: Starting HTTP server...")
            app = create_app(embedding_service)
            if app:
                http_task = asyncio.create_task(run_http_server(app))

        # ── Ready ─────────────────────────────────────────────────────
        logger.info("=" * 70)
        logger.info("✅ Service ready!")
        if RABBITMQ_HOST:
            logger.info(f"🔌 RabbitMQ: Consuming from {REQUEST_QUEUE} (auto-reconnect)")
        if ENABLE_HTTP_API:
            logger.info(f"🌐 Swagger: http://{HTTP_HOST}:{HTTP_PORT}/docs")
        logger.info("=" * 70)

        # Keep running
        if http_task:
            await http_task
        elif consumer_task:
            await consumer_task
        else:
            await asyncio.Future()

    except KeyboardInterrupt:
        logger.info("🛑 Shutting down...")
    except Exception as e:
        logger.error(f"❌ Fatal error: {e}", exc_info=True)
        if not ENABLE_HTTP_API and not RABBITMQ_HOST:
            sys.exit(1)


if __name__ == "__main__":
    asyncio.run(main())