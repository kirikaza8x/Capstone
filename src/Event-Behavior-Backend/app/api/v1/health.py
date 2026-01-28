from fastapi import APIRouter
from app.services.redis_cache import test_connection
router = APIRouter(prefix="/api/v1", tags=["logging"])

@router.post("/health-check")
async def health_check():
    return {"status": "healthy"}
@router.get("/test-redis")
async def check_redis():
    result = await test_connection()
    return result
     