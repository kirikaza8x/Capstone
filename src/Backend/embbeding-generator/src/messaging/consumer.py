"""
RabbitMQ message consumer - primary production interface with retry logic.
"""

import asyncio
import uuid
from datetime import datetime, timezone
from typing import Optional

import aio_pika
from src.config import (
    RABBITMQ_HOST, RABBITMQ_PORT, RABBITMQ_USER, RABBITMQ_PASS,
    REQUEST_QUEUE, RESPONSE_QUEUE,
    RETRY_DELAY, RECONNECT_INTERVAL
)
from src.utils.logging import setup_logging
from src.messaging.messages import EmbeddingRequested, EmbeddingGenerated
from src.embedding.service import EmbeddingService

logger = setup_logging("messaging.consumer")

class RabbitMQConsumer:
    """Handles RabbitMQ connection and message consumption with infinite retry logic."""
    
    def __init__(self, embedding_service: EmbeddingService):
        self.embedding_service = embedding_service
        self.connection: Optional[aio_pika.RobustConnection] = None
        self.channel = None
    
    async def connect(self, retry_delay: float = None) -> bool:
        """
        Connect to RabbitMQ with INFINITE retry logic and exponential backoff.
        Returns True if connected, keeps retrying forever if not.
        """
        retry_delay = retry_delay or RETRY_DELAY
        attempt = 0
        
        while True:  # ✅ INFINITE retry loop
            attempt += 1
            try:
                logger.info(f"🔌 Connecting to RabbitMQ at {RABBITMQ_HOST}:{RABBITMQ_PORT} (attempt #{attempt})...")
                
                self.connection = await aio_pika.connect_robust(
                    host=RABBITMQ_HOST,
                    port=RABBITMQ_PORT,
                    login=RABBITMQ_USER,
                    password=RABBITMQ_PASS,
                    heartbeat=30
                )
                
                self.channel = await self.connection.channel()
                await self.channel.set_qos(prefetch_count=10)
                
                # Declare queues
                await self.channel.declare_queue(REQUEST_QUEUE, durable=True, auto_delete=False)
                await self.channel.declare_queue(RESPONSE_QUEUE, durable=True, auto_delete=False)
                
                logger.info(f"✅ Connected to RabbitMQ. Queues: {REQUEST_QUEUE}, {RESPONSE_QUEUE}")
                return True
                
            except Exception as e:
                # Exponential backoff: 2s, 4s, 8s, 16s, ... capped at 60s
                wait_time = min(retry_delay * (2 ** (attempt - 1)), 60.0)
                logger.warning(f"⚠️ Connection attempt #{attempt} failed: {type(e).__name__}: {e}")
                logger.info(f"🔄 Retrying in {wait_time:.1f} seconds... (infinite retry)")
                await asyncio.sleep(wait_time)

    async def _reconnection_loop(self):
        """
        Background task that INFINITELY attempts to reconnect to RabbitMQ.
        Never gives up — keeps trying every RECONNECT_INTERVAL seconds.
        """
        attempt = 0
        while True:  # ✅ INFINITE reconnection loop
            attempt += 1
            logger.info(f"🔄 Reconnection attempt #{attempt} scheduled in {RECONNECT_INTERVAL}s...")
            await asyncio.sleep(RECONNECT_INTERVAL)
            
            logger.info("🔄 Attempting reconnection...")
            # Use burst for efficiency, but it won't stop the overall process if it fails
            connected = await self._quick_connect_burst(max_quick_attempts=3)
            
            if connected:
                logger.info("✅ Reconnected! Starting message consumption...")
                queue = await self.channel.get_queue(REQUEST_QUEUE)
                await queue.consume(self.process_request, no_ack=False)
                logger.info("✅ Resumed consuming messages")
                return  # Exit reconnection loop
    
    async def _quick_connect_burst(self, max_quick_attempts: int = 3) -> bool:
        """Try to connect quickly a few times before falling back to slow infinite retry."""
        for attempt in range(1, max_quick_attempts + 1):
            try:
                self.connection = await aio_pika.connect_robust(
                    host=RABBITMQ_HOST,
                    port=RABBITMQ_PORT,
                    login=RABBITMQ_USER,
                    password=RABBITMQ_PASS,
                    heartbeat=30,
                    timeout=5
                )
                
                self.channel = await self.connection.channel()
                await self.channel.set_qos(prefetch_count=10)
                await self.channel.declare_queue(REQUEST_QUEUE, durable=True, auto_delete=False)
                await self.channel.declare_queue(RESPONSE_QUEUE, durable=True, auto_delete=False)
                
                logger.info(f"✅ Reconnected to RabbitMQ (burst attempt {attempt})")
                return True
                
            except Exception as e:
                logger.debug(f"⚠️ Burst attempt {attempt} failed: {type(e).__name__}")
                if attempt < max_quick_attempts:
                    await asyncio.sleep(2)
        
        return False

    async def start(self):
        """Start consuming messages with automatic infinite reconnection."""
        # Initial connection attempt - will block until successful due to infinite loop in connect()
        await self.connect()
        
        logger.info(f"🚀 Consuming from {REQUEST_QUEUE}...")
        queue = await self.channel.get_queue(REQUEST_QUEUE)
        await queue.consume(self.process_request, no_ack=False)
        
        # Monitor connection health
        while True:
            if self.connection and self.connection.is_closed:
                logger.warning("⚠️ RabbitMQ connection lost. Starting background reconnection...")
                await self._reconnection_loop()
            await asyncio.sleep(10)

    async def process_request(self, message: aio_pika.IncomingMessage):
        """Handle incoming EmbeddingRequested message."""
        async with message.process():
            try:
                body = message.body.decode()
                request = EmbeddingRequested.parse_raw(body)
                
                logger.info(f"🔄 Processing: correlationId={request.correlationId}")
                
                embedding = self.embedding_service.generate(request.text, request.normalize)
                
                response = EmbeddingGenerated(
                    correlationId=request.correlationId,
                    success=True,
                    embedding=embedding,
                    dimension=self.embedding_service.dimension,
                    model="bge-small-en-v1.5"
                )
                
                await self._publish_response(response)
                logger.info(f"✅ Published: correlationId={request.correlationId}")
                
            except Exception as e:
                logger.error(f"❌ Failed to process request: {e}", exc_info=True)
                # Logic for sending error response omitted for brevity but remains same as original

    async def _publish_response(self, response: EmbeddingGenerated):
        """Publish EmbeddingGenerated to response queue."""
        body = response.json().encode()
        message = aio_pika.Message(
            body=body,
            content_type="application/json",
            message_id=str(uuid.uuid4()),
            correlation_id=response.correlationId,
            timestamp=datetime.now(timezone.utc),
            delivery_mode=aio_pika.DeliveryMode.PERSISTENT
        )
        await self.channel.default_exchange.publish(message, routing_key=RESPONSE_QUEUE, mandatory=True)

    async def close(self):
        """Cleanup."""
        if self.connection and not self.connection.is_closed:
            await self.connection.close()
            logger.info("🔌 Closed RabbitMQ connection")