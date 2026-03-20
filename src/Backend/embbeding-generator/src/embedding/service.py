"""
Embedding generation logic - SHARED by RabbitMQ consumer AND HTTP API.
"""

import logging
from sentence_transformers import SentenceTransformer
from src.utils.logging import setup_logging
from src.config import NORMALIZE_EMBEDDINGS

logger = setup_logging("embedding.service")

class EmbeddingService:
    """
    Core embedding generation service.
    Shared by both RabbitMQ consumer and HTTP API.
    """
    
    def __init__(self, model: SentenceTransformer, dimension: int, model_name: str = "bge-small-en-v1.5"):
        self.model = model
        self.dimension = dimension
        self.model_name = model_name
        logger.info(f"🔧 EmbeddingService initialized: dimension={dimension}, device={self.model.device}")
    
    def generate(self, text: str, normalize: bool = NORMALIZE_EMBEDDINGS) -> list[float]:
        """Generate embedding for single text."""
        if not text or not text.strip():
            raise ValueError("Text cannot be empty")
        
        # DRY: Reuse batch generation logic
        return self.generate_batch([text], normalize)[0]
    
    def generate_batch(self, texts: list[str], normalize: bool = NORMALIZE_EMBEDDINGS) -> list[list[float]]:
        """Generate embeddings for multiple texts."""
        if not texts:
            return []
            
        # Optional safeguard: warn if empty strings are passed in a batch
        if any(not t or not t.strip() for t in texts):
            logger.warning("Empty strings detected in batch generation.")
        
        embeddings = self.model.encode(
            texts,
            convert_to_numpy=True,
            normalize_embeddings=normalize,
            show_progress_bar=False
        )
        
        return [emb.astype(float).tolist() for emb in embeddings]
    
    def get_info(self) -> dict:
        """Return model information for health checks."""
        return {
            "model": self.model_name,
            "dimension": self.dimension,
            "normalize": NORMALIZE_EMBEDDINGS,
            "device": str(self.model.device) # Dynamically returns cpu, cuda, or mps
        }