"""
HTTP API request/response schemas (for Swagger/testing).
"""

from typing import List
from pydantic import BaseModel, Field

class GenerateEmbeddingRequest(BaseModel):
    text: str = Field(..., min_length=1, max_length=512, examples=["machine learning"])
    normalize: bool = Field(True, description="L2 normalize for cosine similarity")

class GenerateEmbeddingResponse(BaseModel):
    success: bool
    text: str
    embedding: List[float]
    dimension: int
    normalized: bool
    model: str

class BatchEmbeddingRequest(BaseModel):
    texts: List[str] = Field(..., min_length=1, max_length=50, examples=[["machine learning", "deep learning"]])
    normalize: bool = Field(True)

class BatchEmbeddingResponse(BaseModel):
    success: bool
    count: int
    embeddings: List[List[float]]
    dimension: int
    normalized: bool
    model: str

class HealthResponse(BaseModel):
    status: str
    service: str
    model: dict
    rabbitmq: dict
    http: dict