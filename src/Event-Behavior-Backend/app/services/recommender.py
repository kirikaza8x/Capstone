# app/services/recommender.py
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession
from datetime import datetime, timedelta, timezone
from collections import defaultdict
from app.models.interaction import UserInteraction
from app.services.redis_cache import redis_client, get_json, set_json

async def get_user_interest_profile(user_id: int, db: AsyncSession, days: int = 90) -> dict:
    cache_key = f"user_profile:{user_id}"
    cached = await get_json(cache_key)
    if cached:
        return cached

    # Use timezone-aware datetime
    cutoff = datetime.now(timezone.utc) - timedelta(days=days)
    result = await db.execute(
        select(UserInteraction)
        .where(UserInteraction.user_id == user_id)
        .where(UserInteraction.occurred_at >= cutoff)
        .order_by(UserInteraction.occurred_at.desc())
        .limit(200)
    )
    logs = result.scalars().all()

    scores = defaultdict(float)
    for log in logs:
        if log.category:
            weight = {
                'purchase': 6.0,
                'register': 4.0,
                'click': 2.5,
                'view': 1.0,
                'search': 1.5
            }.get(log.action_type, 1.0)

            # Fixed subtraction – both aware
            days_ago = (datetime.now(timezone.utc) - log.occurred_at).days
            decay = max(0, 1 - days_ago / days)

            scores[log.category] += weight * decay

    top_categories = sorted(scores.items(), key=lambda x: x[1], reverse=True)[:5]

    profile = {
        "top_categories": [cat for cat, _ in top_categories],
        "category_scores": dict(scores),
        "total_actions": len(logs),
        "last_active": logs[0].occurred_at.isoformat() if logs else None
    }

    await set_json(cache_key, profile, ttl=3600)
    return profile