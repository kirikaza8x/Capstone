"""
Embedding generation logic - SHARED by RabbitMQ consumer AND HTTP API.
"""

import logging
from typing import List
from sentence_transformers import SentenceTransformer
from src.utils.logging import setup_logging
from src.config import NORMALIZE_EMBEDDINGS

logger = setup_logging("embedding.service")

class EmbeddingService:
    """
    Core embedding generation service.
    Shared by both RabbitMQ consumer and HTTP API.
    """
    
    def __init__(self, model: SentenceTransformer, dimension: int):
        self.model = model
        self.dimension = dimension
        logger.info(f"🔧 EmbeddingService initialized: dimension={dimension}")
    
    def generate(self, text: str, normalize: bool = NORMALIZE_EMBEDDINGS) -> List[float]:
        """Generate embedding for single text."""
        if not text or not text.strip():
            raise ValueError("Text cannot be empty")
        
        embedding = self.model.encode(
            [text],
            convert_to_numpy=True,
            normalize_embeddings=normalize,
            show_progress_bar=False
        )[0]
        
        return embedding.astype(float).tolist()
    
    def generate_batch(self, texts: List[str], normalize: bool = NORMALIZE_EMBEDDINGS) -> List[List[float]]:
        """Generate embeddings for multiple texts."""
        if not texts:
            return []
        
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
            "model": "bge-small-en-v1.5",
            "dimension": self.dimension,
            "normalize": NORMALIZE_EMBEDDINGS,
            "device": "cpu"
        }