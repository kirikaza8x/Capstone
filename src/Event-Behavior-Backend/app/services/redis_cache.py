# app/services/redis.py
import redis.asyncio as redis
from app.config import settings
import json
from typing import Any, Optional

redis_client = redis.from_url(
    settings.REDIS_URL,
    decode_responses=True,           # return strings instead of bytes
    socket_timeout=5,
    socket_connect_timeout=5
)

async def test_connection() -> dict:
    try:
        pong = await redis_client.ping()
        info = await redis_client.info()
        return {
            "status": "connected",
            "pong": pong,
            "redis_version": info.get("redis_version", "unknown"),
            "uptime_days": info.get("uptime_in_days", "unknown")
        }
    except Exception as e:
        return {"status": "failed", "error": str(e)}

async def set_json(key: str, value: Any, ttl: int = 3600):
    if isinstance(value, (dict, list)):
        value = json.dumps(value)
    await redis_client.setex(key, ttl, value)

async def get_json(key: str) -> Optional[Any]:
    value = await redis_client.get(key)
    if value is None:
        return None
    try:
        return json.loads(value)
    except json.JSONDecodeError:
        return value