# app/main.py
from contextlib import asynccontextmanager
from fastapi import FastAPI

# Import the router here (this line was missing or wrong)
from app.api.v1.logs import router as logs  

from app.database import engine, Base

@asynccontextmanager
async def lifespan(app: FastAPI):
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    yield

app = FastAPI(
    title="Event AI Recommendation Backend",
    lifespan=lifespan
)

app.include_router(logs, prefix="/api/v1")  