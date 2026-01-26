from fastapi import APIRouter
from app.services import gemini
router = APIRouter(prefix="/api/v1", tags=["ai_assistant"])

@router.post("/ai-assistant")
async def ai_assistant(prompt: str):
    response = await gemini.generate_text(prompt)
    return {"response": response}