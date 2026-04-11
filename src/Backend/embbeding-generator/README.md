 
# Embedding Service

Python service for generating text embeddings with TWO interfaces:

## 🔌 Production: MassTransit/RabbitMQ
- C# sends via `embedding.requests` queue
- Python responds via `embedding.responses` queue

## 🧪 Testing: HTTP + Swagger
- Swagger UI: http://localhost:8001/docs
- Health: http://localhost:8001/health
- Generate: POST http://localhost:8001/embeddings/generate

## Quick Start

```bash
# 1. Install
pip install -r requirements.txt

# 2. Download model (one-time)
python scripts/download_model.py

# 3. Run
python -m src.main

# 4. Test
# Open: http://localhost:8001/docs