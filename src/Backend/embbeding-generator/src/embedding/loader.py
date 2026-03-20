"""
Model loading logic - supports both old (pytorch) and new (safetensors) formats.
"""

import logging
from pathlib import Path
from sentence_transformers import SentenceTransformer
from src.utils.logging import setup_logging

logger = setup_logging("embedding.loader")

class ModelLoader:
    """Handles model loading and validation."""
    
    def __init__(self, model_path: Path):
        self.model_path = model_path
        self.model = None
        self.dimension = None
    
    def validate(self) -> bool:
        """Check if model files exist (supports both old and new formats)."""
        if not self.model_path.exists():
            logger.error(f"❌ Model path not found: {self.model_path}")
            return False
        
        # ✅ Check for NEW format (safetensors + subfolders)
        new_format_files = ["model.safetensors", "config.json", "tokenizer.json"]
        new_format_folders = ["1_Pooling", "2_Normalize"]
        
        has_new_format = all((self.model_path / f).exists() for f in new_format_files)
        has_new_folders = all((self.model_path / folder).exists() for folder in new_format_folders)
        
        if has_new_format and has_new_folders:
            logger.info(f"✅ Model validation passed (new format): {self.model_path}")
            return True
        
        # ✅ Check for OLD format (pytorch bin)
        old_format_files = ["pytorch_model.bin", "config.json", "tokenizer.json"]
        has_old_format = all((self.model_path / f).exists() for f in old_format_files)
        
        if has_old_format:
            logger.info(f"✅ Model validation passed (old format): {self.model_path}")
            return True
        
        # ❌ Neither format found
        logger.error(f"❌ Required files missing in: {self.model_path}")
        found_files = [f.name for f in self.model_path.iterdir()]
        logger.error(f"📁 Found: {found_files}")
        return False
    
    def load(self, device: str = "cpu") -> SentenceTransformer:
        """Load the model into memory."""
        if not self.validate():
            raise FileNotFoundError(f"Model not found at {self.model_path}")
        
        logger.info(f"📦 Loading model from {self.model_path} onto {device}...")
        self.model = SentenceTransformer(str(self.model_path), device=device)
        self.dimension = self.model.get_sentence_embedding_dimension()
        
        logger.info(f"✅ Model loaded: dimension={self.dimension}")
        return self.model
    
    def get_dimension(self) -> int:
        """Get model embedding dimension."""
        if self.dimension is None:
            raise RuntimeError("Model not loaded yet")
        return self.dimension