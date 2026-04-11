# ============================================
# ECR — Elastic Container Registry for storing Docker images
# ============================================

# Backend API repository
resource "aws_ecr_repository" "backend_api" {
  name                 = "${var.project}-${var.environment}-backend-api"
  image_tag_mutability = "MUTABLE"  # Enable overwriting tag 'latest'
  force_delete         = true       # Allow deleting repo when terraform destroy

  image_scanning_configuration {
    scan_on_push = true  # Scan vulnerability when pushing image
  }

  tags = {
    Name        = "${var.project}-${var.environment}-backend-api"
    Project     = var.project
    Environment = var.environment
  }
}

# Embedding Service repository
resource "aws_ecr_repository" "embedding" {
  name                 = "${var.project}-${var.environment}-embedding"
  image_tag_mutability = "MUTABLE"
  force_delete         = true

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name        = "${var.project}-${var.environment}-embedding"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# ECR Lifecycle Policy — delete old images, keep recent ones
# ============================================

resource "aws_ecr_lifecycle_policy" "backend_api" {
  repository = aws_ecr_repository.backend_api.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep last 5 images, expire older"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 5
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}

resource "aws_ecr_lifecycle_policy" "embedding" {
  repository = aws_ecr_repository.embedding.name

  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep last 3 images, expire older"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 3
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}
