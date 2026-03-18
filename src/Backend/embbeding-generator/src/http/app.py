"""
FastAPI HTTP server for testing/debugging with Swagger UI.
"""

import time
from fastapi import FastAPI, HTTPException, Request
from src.config import HTTP_HOST, HTTP_PORT, ENABLE_HTTP_API, RABBITMQ_HOST
from src.utils.logging import setup_logging
from src.http.schemas import (
    GenerateEmbeddingRequest, GenerateEmbeddingResponse,
    BatchEmbeddingRequest, BatchEmbeddingResponse, HealthResponse
)
from src.embedding.service import EmbeddingService

logger = setup_logging("http.app")

def create_app(embedding_service: EmbeddingService) -> FastAPI:
    """Create FastAPI app with embedding endpoints."""
    
    if not ENABLE_HTTP_API:
        logger.warning("⚠️  HTTP API disabled by config")
        return None
    
    app = FastAPI(
        title="Embedding Service (Testing)",
        description="HTTP endpoints for manual testing. Production uses RabbitMQ.",
        version="1.0.0",
        docs_url="/docs",
        redoc_url="/redoc",
        openapi_url="/openapi.json"
    )
    
    @app.get("/health", response_model=HealthResponse, tags=["Health"])
    async def health_check():
        """Health check endpoint with service status."""
        logger.debug("Health check requested")
        return HealthResponse(
            status="healthy",
            service="embedding-service",
            model=embedding_service.get_info(),
            rabbitmq={
                "enabled": bool(RABBITMQ_HOST),
                "host": RABBITMQ_HOST,
                "note": "Consumer runs with auto-reconnect"
            },
            http={"host": HTTP_HOST, "port": HTTP_PORT}
        )
    
    @app.get("/model/info", tags=["Health"])
    async def model_info():
        """Return model information."""
        info = embedding_service.get_info()
        logger.info(f"Model info requested: {info.get('model_name', 'unknown')}")
        return info
    
    @app.post("/embeddings/generate", response_model=GenerateEmbeddingResponse, tags=["Embeddings"])
    async def generate_embedding(request: GenerateEmbeddingRequest):
        """Generate embedding for single text."""
        start_time = time.perf_counter()
        # Log the incoming request (truncated text for brevity)
        preview = (request.text[:50] + '...') if len(request.text) > 50 else request.text
        logger.info(f"📥 Generating embedding | length: {len(request.text)} chars | text: '{preview}'")
        
        try:
            embedding = embedding_service.generate(request.text, request.normalize)
            
            duration = time.perf_counter() - start_time
            logger.info(f"✅ Embedding generated in {duration:.4f}s | dim: {len(embedding)}")
            
            return GenerateEmbeddingResponse(
                success=True,
                text=request.text,
                embedding=embedding,
                dimension=len(embedding),
                normalized=request.normalize,
                model="bge-small-en-v1.5"
            )
        except Exception as e:
            logger.error(f"❌ Failed to generate embedding: {str(e)}", exc_info=True)
            raise HTTPException(status_code=500, detail="Internal server error during embedding generation")
    
    @app.post("/embeddings/batch", response_model=BatchEmbeddingResponse, tags=["Embeddings"])
    async def generate_embeddings_batch(request: BatchEmbeddingRequest):
        """Generate embeddings for multiple texts."""
        start_time = time.perf_counter()
        batch_size = len(request.texts)
        logger.info(f"📥 Batch embedding started | size: {batch_size} items")
        
        try:
            embeddings = embedding_service.generate_batch(request.texts, request.normalize)
            
            duration = time.perf_counter() - start_time
            logger.info(f"✅ Batch completed | size: {batch_size} | time: {duration:.4f}s")
            
            return BatchEmbeddingResponse(
                success=True,
                count=len(embeddings),
                embeddings=embeddings,
                dimension=embedding_service.dimension,
                normalized=request.normalize,
                model="bge-small-en-v1.5"
            )
        except Exception as e:
            logger.error(f"❌ Batch embedding failed for {batch_size} items: {str(e)}", exc_info=True)
            raise HTTPException(status_code=500, detail="Internal server error during batch processing")
    
    @app.get("/", tags=["Root"])
    async def root():
        """Root endpoint with service info."""
        return {
            "service": "embedding-service",
            "description": "HTTP testing interface (production uses RabbitMQ)",
            "swagger": "/docs",
            "health": "/health",
            "model_info": "/model/info"
        }
    
    logger.info(f"🚀 HTTP API ready: http://{HTTP_HOST}:{HTTP_PORT}/docs")
    return app