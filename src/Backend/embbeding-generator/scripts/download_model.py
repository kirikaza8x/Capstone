"""
Downloads model snapshot + exports to ONNX for lightweight inference.
No PyTorch needed at runtime after this step.

Usage: python scripts/download_model.py
"""

import os
import sys
from pathlib import Path
from huggingface_hub import snapshot_download
from huggingface_hub.utils import HfHubHTTPError, RepositoryNotFoundError
from optimum.onnxruntime import ORTModelForFeatureExtraction
from transformers import AutoTokenizer

MODEL_ID    = os.getenv("MODEL_ID", "BAAI/bge-small-en-v1.5")
DEFAULT_OUT = Path(__file__).parent.parent / "models" / "bge-small-en-v1.5"
OUTPUT_DIR  = Path(os.getenv("MODEL_PATH", DEFAULT_OUT))

IGNORE_PATTERNS = [
    "*.msgpack", "*.h5",
    "flax_model*", "tf_model*",
    "rust_model.ot", "coreml/*", "*.ot",
    "README.md",
]

def download_snapshot():
    print("=" * 60)
    print("📥 Step 1: Downloading model snapshot")
    print(f"   Model      : {MODEL_ID}")
    print(f"   Destination: {OUTPUT_DIR}")
    print("=" * 60)

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    try:
        snapshot_download(
            repo_id=MODEL_ID,
            local_dir=str(OUTPUT_DIR),
            ignore_patterns=IGNORE_PATTERNS,
        )
        print("✅ Snapshot downloaded")
    except RepositoryNotFoundError:
        print(f"❌ Model '{MODEL_ID}' not found.", file=sys.stderr)
        sys.exit(1)
    except HfHubHTTPError as e:
        print(f"❌ Network error: {e}", file=sys.stderr)
        sys.exit(1)


def export_onnx():
    print("=" * 60)
    print("⚙️  Step 2: Exporting to ONNX format")
    print("=" * 60)

    try:
        ort_model = ORTModelForFeatureExtraction.from_pretrained(
            str(OUTPUT_DIR),
            export=True,
        )
        ort_model.save_pretrained(str(OUTPUT_DIR))

        tokenizer = AutoTokenizer.from_pretrained(str(OUTPUT_DIR))
        tokenizer.save_pretrained(str(OUTPUT_DIR))

        print("✅ ONNX export complete")
    except Exception as e:
        print(f"❌ ONNX export failed: {type(e).__name__}: {e}", file=sys.stderr)
        sys.exit(1)


def cleanup():
    print("=" * 60)
    print("🧹 Step 3: Cleaning up unnecessary files")
    print("=" * 60)

    remove_patterns = [
        "pytorch_model.bin",   # 134MB — replaced by ONNX
        "model.safetensors",   # 133MB — replaced by ONNX
        "*.metadata",          # HF cache metadata
        ".gitattributes",
        ".gitignore",
        "README.md",
    ]

    removed_mb = 0
    for pattern in remove_patterns:
        for f in OUTPUT_DIR.rglob(pattern):
            size_mb = f.stat().st_size / 1024 / 1024
            f.unlink()
            removed_mb += size_mb
            print(f"   🗑️  Removed: {f.name} ({size_mb:.1f} MB)")

    print(f"✅ Cleanup done — freed {removed_mb:.1f} MB")


def verify():
    print("=" * 60)
    print("🔍 Step 4: Verifying output")

    files   = [f.name for f in OUTPUT_DIR.rglob("*") if f.is_file()]
    size_mb = sum(f.stat().st_size for f in OUTPUT_DIR.rglob("*") if f.is_file()) / 1024 / 1024

    print(f"   Files : {files}")
    print(f"   Size  : {size_mb:.1f} MB")

    if not (OUTPUT_DIR / "model.onnx").exists():
        print("❌ model.onnx not found — export may have failed", file=sys.stderr)
        sys.exit(1)

    print("✅ All good — ready for inference")
    print("=" * 60)


if __name__ == "__main__":
    download_snapshot()
    export_onnx()
    cleanup()
    verify()