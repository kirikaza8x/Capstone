# app/services/recommender.py
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession
from datetime import datetime, timedelta
from collections import defaultdict
from app.models.interaction import UserInteraction
from app.services.redis_cache import redis_client, get_json, set_json

async def get_user_interest_profile(user_id: int, db: AsyncSession, days: int = 90) -> dict:
    """
    Aggregate user behavior from logs into a profile.
    - Weights: purchase=6.0, register=4.0, click=2.5, view=1.0, search=1.5
    - Recency decay: linear over 90 days
    - Cache result in Redis for 1 hour
    """
    cache_key = f"user_profile:{user_id}"
    cached = await get_json(cache_key)
    if cached:
        return cached

    cutoff = datetime.utcnow() - timedelta(days=days)
    result = await db.execute(
        select(UserInteraction)
        .where(UserInteraction.user_id == user_id)
        .where(UserInteraction.occurred_at >= cutoff)
        .order_by(UserInteraction.occurred_at.desc())
        .limit(200)  # limit to last 200 interactions
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

            days_ago = (datetime.utcnow() - log.occurred_at).days
            decay = max(0, 1 - days_ago / days)

            scores[log.category] += weight * decay

    top_categories = sorted(scores.items(), key=lambda x: x[1], reverse=True)[:5]

    profile = {
        "top_categories": [cat for cat, _ in top_categories],
        "category_scores": dict(scores),
        "total_actions": len(logs),
        "last_active": logs[0].occurred_at.isoformat() if logs else None
    }

    # Cache for 1 hour
    await set_json(cache_key, profile, ttl=3600)
    return profile