"""
Message contracts for RabbitMQ - must match C# EmbeddingContracts.cs
"""

import uuid
from datetime import datetime, timezone
from typing import List, Optional
from pydantic import BaseModel, Field, validator, root_validator

class EmbeddingRequested(BaseModel):
    """Request: C# asks Python to generate an embedding."""
    
    correlationId: str
    text: str = Field(..., min_length=1, max_length=512)
    normalize: bool = True
    requestedAt: Optional[str] = None
    
    @validator('correlationId')
    def validate_uuid(cls, v):
        try:
            uuid.UUID(v)
            return v
        except ValueError:
            raise ValueError('correlationId must be a valid UUID')

class EmbeddingGenerated(BaseModel):
    """Response: Python returns the generated embedding (or error)."""
    
    correlationId: str
    success: bool
    embedding: Optional[List[float]] = None
    dimension: int = 384
    model: str = "bge-small-en-v1.5"
    error: Optional[str] = None
    processedAt: str = Field(default_factory=lambda: datetime.now(timezone.utc).isoformat())
    
    @validator('correlationId')
    def validate_uuid(cls, v):
        try:
            uuid.UUID(v)
            return v
        except ValueError:
            raise ValueError('correlationId must be a valid UUID')
    
    @root_validator
    def validate_success_or_error(cls, values):
        success = values.get('success')
        embedding = values.get('embedding')
        error = values.get('error')
        
        if success and embedding is None:
            raise ValueError('embedding is required when success=true')
        if not success and error is None:
            raise ValueError('error is required when success=false')
        return values