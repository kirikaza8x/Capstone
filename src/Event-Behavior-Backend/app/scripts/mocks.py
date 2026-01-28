import asyncio
from datetime import datetime, timedelta
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, func
from app.database import SessionLocal, engine, Base
from app.models.interaction import UserInteraction

async def create_tables_if_needed():
    """Create tables if they don't exist (dev only)"""
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)

async def clear_existing_logs():
    """Optional: clear old logs for clean test"""
    async with SessionLocal() as db:
        await db.execute(UserInteraction.__table__.delete())
        await db.commit()
        print("Cleared existing logs.")

async def insert_mock_logs():
    async with SessionLocal() as db:
        # Define 3 mock users with different interests
        mock_users = [
            {
                "user_id": 1001,
                "name": "Tech Enthusiast",
                "behavior": [
                    ("view", "tech", 15, 1.0),
                    ("click", "tech", 8, 2.5),
                    ("register", "tech", 3, 4.0),
                    ("purchase", "tech", 2, 6.0),
                    ("view", "seminar", 5, 1.0),
                ]
            },
            {
                "user_id": 1002,
                "name": "Music Lover",
                "behavior": [
                    ("purchase", "concert", 4, 6.0),
                    ("click", "concert", 10, 2.5),
                    ("view", "concert", 20, 1.0),
                    ("search", "indie music", 3, 1.5),
                    ("view", "festival", 6, 1.0),
                ]
            },
            {
                "user_id": 1003,
                "name": "Foodie",
                "behavior": [
                    ("view", "food", 12, 1.0),
                    ("search", "food festival HCMC", 5, 1.5),
                    ("register", "food", 2, 4.0),
                    ("click", "food", 6, 2.5),
                    ("purchase", "food", 1, 6.0),
                ]
            },
        ]

        now = datetime.utcnow()

        for user in mock_users:
            user_id = user["user_id"]
            print(f"Generating logs for user {user_id} ({user['name']})")

            for action_type, category, count, _ in user["behavior"]:
                for i in range(count):
                    # Spread over last 60 days, random hour offset
                    days_ago = i % 60
                    hours_offset = i % 24
                    occurred = now - timedelta(days=days_ago, hours=hours_offset)

                    log = UserInteraction(
                        user_id=user_id,
                        action_type=action_type,
                        category=category,
                        event_id=1000 + i,  # fake event ID
                        target=f"{action_type}_{category}_{i}",
                        metadata={"source": "mock", "device": "mobile" if i % 2 == 0 else "web"},
                        occurred_at=occurred
                    )
                    db.add(log)

        await db.commit()
        print("Mock logs inserted successfully!")

        # Quick stats check
        result = await db.execute(select(func.count()).select_from(UserInteraction))
        total_logs = result.scalar()
        print(f"Total logs in DB: {total_logs}")

if __name__ == "__main__":
    print("Starting mock data generation...")
    asyncio.run(create_tables_if_needed())
    # Uncomment if you want a clean slate each run:
    # asyncio.run(clear_existing_logs())
    asyncio.run(insert_mock_logs())
    print("Mock data generation completed.")