from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession
from app.services.recommender import get_user_interest_profile
from app.database import get_db  

router = APIRouter(prefix="/api/v1", tags=["test"])

@router.get("/test-profile/{user_id}")
async def test_profile(user_id: int, db: AsyncSession = Depends(get_db)):
    try:
        profile = await get_user_interest_profile(user_id, db)
        return profile
    except Exception as e:
        return {"error": str(e)}
