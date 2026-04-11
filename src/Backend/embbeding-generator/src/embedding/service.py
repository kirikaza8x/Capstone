"""
Embedding generation using ONNX Runtime + mean pooling.
Shared by RabbitMQ consumer AND HTTP API.
"""

import torch
import numpy as np
from transformers import AutoTokenizer
from optimum.onnxruntime import ORTModelForFeatureExtraction
from src.utils.logging import setup_logging
from src.config import NORMALIZE_EMBEDDINGS

logger = setup_logging("embedding.service")

class EmbeddingService:
    """
    ONNX-based embedding service.
    Replaces SentenceTransformer with direct ONNX inference + mean pooling.
    """

    def __init__(
        self,
        model: ORTModelForFeatureExtraction,
        tokenizer: AutoTokenizer,
        dimension: int,
        model_name: str = "bge-small-en-v1.5",
    ):
        self.model     = model
        self.tokenizer = tokenizer
        self.dimension = dimension
        self.model_name = model_name
        logger.info(f"🔧 EmbeddingService (ONNX) initialized: dimension={dimension}")

    def _mean_pool(self, last_hidden_state: torch.Tensor, attention_mask: torch.Tensor) -> np.ndarray:
        """Mean pooling over token embeddings, respecting padding."""
        mask_expanded = attention_mask.unsqueeze(-1).float()
        summed        = (last_hidden_state * mask_expanded).sum(dim=1)
        counts        = mask_expanded.sum(dim=1).clamp(min=1e-9)
        return (summed / counts).detach().numpy()

    def generate(self, text: str, normalize: bool = NORMALIZE_EMBEDDINGS) -> list[float]:
        """Generate embedding for a single text."""
        if not text or not text.strip():
            raise ValueError("Text cannot be empty")
        return self.generate_batch([text], normalize)[0]

    def generate_batch(self, texts: list[str], normalize: bool = NORMALIZE_EMBEDDINGS) -> list[list[float]]:
        """Generate embeddings for multiple texts."""
        if not texts:
            return []

        if any(not t or not t.strip() for t in texts):
            logger.warning("⚠️ Empty strings in batch — results may be incorrect.")

        # Tokenize
        inputs = self.tokenizer(
            texts,
            padding=True,
            truncation=True,
            max_length=512,
            return_tensors="pt",
        )

        # ONNX inference
        with torch.no_grad():
            outputs = self.model(**inputs)

        # Mean pool
        embeddings = self._mean_pool(outputs.last_hidden_state, inputs["attention_mask"])

        # Optional L2 normalize (important for BGE models)
        if normalize:
            norms      = np.linalg.norm(embeddings, axis=1, keepdims=True)
            embeddings = embeddings / np.clip(norms, a_min=1e-9, a_max=None)

        return embeddings.astype(float).tolist()

    def get_info(self) -> dict:
        return {
            "model":     self.model_name,
            "dimension": self.dimension,
            "normalize": NORMALIZE_EMBEDDINGS,
            "backend":   "onnxruntime",
        }