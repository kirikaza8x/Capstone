from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession
from app.database import get_db
from app.models.interaction import UserInteraction
from app.schemas.interaction import LogInteractionCreate

router = APIRouter(prefix="/api/v1", tags=["logging"])

@router.post("/log-interaction")
async def log_interaction(data: LogInteractionCreate, db: AsyncSession = Depends(get_db)):
    try:
        row = UserInteraction(**data.model_dump(exclude_unset=True))
        db.add(row)
        await db.commit()
        await db.refresh(row)
        return {"status": "logged", "id": row.id}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))