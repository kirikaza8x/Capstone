"""
FastAPI HTTP server for testing/debugging with Swagger UI.
"""

from fastapi import FastAPI, HTTPException
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
        logger.warning("⚠️ HTTP API disabled by config")
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
        return embedding_service.get_info()
    
    @app.post("/embeddings/generate", response_model=GenerateEmbeddingResponse, tags=["Embeddings"])
    async def generate_embedding(request: GenerateEmbeddingRequest):
        """
        Generate embedding for single text.
        
        Use this for manual testing. Production uses RabbitMQ.
        """
        try:
            embedding = embedding_service.generate(request.text, request.normalize)
            
            return GenerateEmbeddingResponse(
                success=True,
                text=request.text,
                embedding=embedding,
                dimension=len(embedding),
                normalized=request.normalize,
                model="bge-small-en-v1.5"
            )
        except Exception as e:
            logger.error(f"Failed to generate embedding: {e}", exc_info=True)
            raise HTTPException(status_code=500, detail=str(e))
    
    @app.post("/embeddings/batch", response_model=BatchEmbeddingResponse, tags=["Embeddings"])
    async def generate_embeddings_batch(request: BatchEmbeddingRequest):
        """
        Generate embeddings for multiple texts.
        
        Use this for manual testing. Production uses RabbitMQ.
        """
        try:
            embeddings = embedding_service.generate_batch(request.texts, request.normalize)
            
            return BatchEmbeddingResponse(
                success=True,
                count=len(embeddings),
                embeddings=embeddings,
                dimension=embedding_service.dimension,
                normalized=request.normalize,
                model="bge-small-en-v1.5"
            )
        except Exception as e:
            logger.error(f"Batch embedding failed: {e}", exc_info=True)
            raise HTTPException(status_code=500, detail=str(e))
    
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
    
    logger.info(f"✅ HTTP API ready: http://{HTTP_HOST}:{HTTP_PORT}/docs")
    return app