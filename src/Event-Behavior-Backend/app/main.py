# app/main.py
from contextlib import asynccontextmanager
from fastapi import FastAPI

from app.api.v1.logs import router as logs  
from app.api.v1.health import router as health
from app.api.v1.ai_assistant import router as ai_assistant
from app.database import engine, Base

@asynccontextmanager
async def lifespan(app: FastAPI):
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    yield

app = FastAPI(
    title="Event AI Recommendation Backend",
    #lifespan=lifespan
)

app.include_router(logs, prefix="/api/v1")  
app.include_router(health, prefix="/api/v1")
app.include_router(ai_assistant, prefix="/api/v1")