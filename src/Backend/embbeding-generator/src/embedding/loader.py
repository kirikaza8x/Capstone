"""
Model loader — uses ONNX Runtime for fast, lightweight inference.
No PyTorch dependency at runtime.
"""

from pathlib import Path
from optimum.onnxruntime import ORTModelForFeatureExtraction
from transformers import AutoTokenizer
from src.utils.logging import setup_logging

logger = setup_logging("embedding.loader")

_REQUIRED_FILES = ["model.onnx", "tokenizer.json", "config.json"]


class ModelLoader:
    """Loads ONNX model + tokenizer from local snapshot directory."""

    def __init__(self, model_path: Path):
        self.model_path = model_path
        self.model: ORTModelForFeatureExtraction | None = None
        self.tokenizer: AutoTokenizer | None = None
        self.dimension: int | None = None

    def validate(self) -> bool:
        if not self.model_path.exists():
            logger.error(f"❌ Model path not found: {self.model_path}")
            return False

        missing = [f for f in _REQUIRED_FILES if not (self.model_path / f).exists()]
        if missing:
            logger.error(f"❌ Missing required files: {missing}")
            return False

        logger.info(f"✅ ONNX model validated: {self.model_path}")
        return True

    def load(self, device: str = "cpu") -> tuple[ORTModelForFeatureExtraction, AutoTokenizer]:
        """Returns (ort_model, tokenizer)."""
        if not self.validate():
            raise FileNotFoundError(f"Invalid ONNX model at {self.model_path}")

        logger.info(f"📦 Loading ONNX model from: {self.model_path}")

        try:
            self.model = ORTModelForFeatureExtraction.from_pretrained(
                str(self.model_path),
                local_files_only=True,
            )
            self.tokenizer = AutoTokenizer.from_pretrained(
                str(self.model_path),
                local_files_only=True,
            )
        except Exception as e:
            logger.error(f"❌ Failed to load ONNX model: {type(e).__name__}: {e}")
            raise

        # Derive dimension via test inference
        import torch
        inputs = self.tokenizer("test", return_tensors="pt")
        with torch.no_grad():
            outputs = self.model(**inputs)
        self.dimension = outputs.last_hidden_state.shape[-1]

        logger.info(f"✅ ONNX model loaded: dim={self.dimension}")
        return self.model, self.tokenizer

    def get_dimension(self) -> int:
        if self.dimension is None:
            raise RuntimeError("Model not loaded yet — call load() first")
        return self.dimension