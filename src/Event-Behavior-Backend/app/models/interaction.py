from sqlalchemy import Column, BigInteger, String, JSON, DateTime, func
from app.database import Base

class UserInteraction(Base):
    __tablename__ = "user_interactions"

    id = Column(BigInteger, primary_key=True, autoincrement=True)
    user_id = Column(BigInteger, nullable=True)             # null = anonymous
    event_id = Column(BigInteger, nullable=True)
    action_type = Column(String, nullable=False)
    category = Column(String, nullable=True)
    target = Column(String, nullable=True)
    metadata = Column(JSON, nullable=True)
    occurred_at = Column(DateTime(timezone=True), server_default=func.now())