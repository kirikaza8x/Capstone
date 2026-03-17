"""
Logging configuration.
"""

import logging
import sys
from src.config import LOG_LEVEL

def setup_logging(name: str = "embedding-service") -> logging.Logger:
    """Configure and return a logger instance."""
    
    logger = logging.getLogger(name)
    logger.setLevel(getattr(logging, LOG_LEVEL.upper()))
    
    # Clear existing handlers to avoid duplicates
    logger.handlers = []
    
    # Console handler
    handler = logging.StreamHandler(sys.stdout)
    handler.setLevel(logging.DEBUG)
    
    # Formatter
    formatter = logging.Formatter(
        fmt="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
        datefmt="%Y-%m-%d %H:%M:%S"
    )
    handler.setFormatter(formatter)
    
    logger.addHandler(handler)
    
    # Prevent log propagation to root logger
    logger.propagate = False
    
    return logger