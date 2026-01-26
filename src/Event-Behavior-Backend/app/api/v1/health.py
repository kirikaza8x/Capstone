from fastapi import APIRouter
router = APIRouter(prefix="/api/v1", tags=["logging"])

@router.post("/health-check")
async def health_check():
    return {"status": "healthy"}
     