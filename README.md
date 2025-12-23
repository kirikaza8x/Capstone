# Capstone
# 1. Stop containers and DELETE ALL VOLUMES (Critical for deep clean)
docker-compose down -v

# 2. Clear build cache (Optional, ensures code changes are picked up)
docker builder prune -a -f

# 3. Build and Start everything
docker-compose up --build --force-recreate