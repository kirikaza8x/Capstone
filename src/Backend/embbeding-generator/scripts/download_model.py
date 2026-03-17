"""
One-time script to download the model.
STANDALONE - does not import from src/

Usage: python scripts/download_model.py
"""

from pathlib import Path
from sentence_transformers import SentenceTransformer

# Configuration
MODEL_ID = "BAAI/bge-small-en-v1.5"
OUTPUT_DIR = Path(__file__).parent.parent / "models" / "bge-small-en-v1.5"

def main():
    print("=" * 60)
    print("📥 Downloading Embedding Model")
    print("=" * 60)
    print(f"Model: {MODEL_ID}")
    print(f"Destination: {OUTPUT_DIR}")
    print("=" * 60)
    
    # Create output directory
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    print(f"📁 Created directory: {OUTPUT_DIR}")
    
    # Download and save model
    print("⏳ Downloading model (this may take 2-5 minutes)...")
    model = SentenceTransformer(MODEL_ID)
    model.save(str(OUTPUT_DIR))
    
    # Verify files
    files = [f.name for f in OUTPUT_DIR.iterdir()]
    size_mb = sum(f.stat().st_size for f in OUTPUT_DIR.iterdir()) / 1024 / 1024
    
    print("=" * 60)
    print("✅ Model download complete!")
    print(f"📁 Files: {files}")
    print(f"📦 Size: {size_mb:.1f} MB")
    print("=" * 60)
    print("")
    print("Next step: Run the service with:")
    print("  python -m src.main")
    print("=" * 60)

if __name__ == "__main__":
    main()