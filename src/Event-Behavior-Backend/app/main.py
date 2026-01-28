from contextlib import asynccontextmanager
from fastapi import FastAPI

from app.api.v1.logs import router as logs_router
from app.api.v1.health import router as health_router
from app.api.v1.ai_assistant import router as ai_assistant_router
from app.api.v1.tests import router as tests_router

from app.database import engine, Base
from app.config import settings
from app.scripts.mocks import insert_mock_logs,clear_existing_logs


@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: create tables (dev only – use Alembic in production)
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)

    # Run mock data generator only in dev environment
    if settings.ENV == "dev":
        await clear_existing_logs()
        await insert_mock_logs()

    yield


app = FastAPI(
    title="Event AI Recommendation Backend",
    lifespan=lifespan
)

# Include routers – only once each
app.include_router(logs_router, prefix="/api/v1", tags=["logging"])
app.include_router(health_router, prefix="/api/v1", tags=["health"])
app.include_router(ai_assistant_router, prefix="/api/v1", tags=["ai_assistant"])
app.include_router(tests_router, prefix="/api/v1", tags=["test"])
