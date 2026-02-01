from pydantic import BaseModel
from typing import Optional, Dict, Any

class LogInteractionCreate(BaseModel):
    user_id: Optional[int] = None
    event_id: Optional[int] = None
    action_type: str
    category: Optional[str] = None
    target: Optional[str] = None
    metadata: Optional[Dict[str, Any]] = None