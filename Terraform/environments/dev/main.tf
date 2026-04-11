# ============================================
# AIPromo — Development Environment
# ============================================

terraform {
  required_version = ">= 1.6.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  backend "s3" {
    bucket = "aipromo-terraform-state"
    key    = "dev/terraform.tfstate"
    region = "ap-southeast-1"
  }
}

provider "aws" {
  region = var.aws_region
}

# ============================================
# VPC — Network
# ============================================

module "vpc" {
  source = "../../modules/vpc"

  project     = var.project
  environment = var.environment
  vpc_cidr    = var.vpc_cidr
}

# ============================================
# S3 — Media Storage
# ============================================

module "s3" {
  source = "../../modules/s3"

  project     = var.project
  environment = var.environment
}

# ============================================
# ECR — Docker Image Repositories
# ============================================

module "ecr" {
  source = "../../modules/ecr"

  project     = var.project
  environment = var.environment
}

# ============================================
# ECS — Container Orchestration
# ============================================

module "ecs" {
  source = "../../modules/ecs"

  project                      = var.project
  environment                  = var.environment
  aws_region                   = var.aws_region
  vpc_id                       = module.vpc.vpc_id
  ec2_security_group_id        = module.ec2.security_group_id
  ecr_repository_url           = module.ecr.backend_api_repository_url
  embedding_ecr_repository_url = module.ecr.embedding_repository_url
  s3_bucket_arn                = module.s3.bucket_arn

  # Backend API settings
  backend_port    = var.backend_port
  backend_cpu     = "512"
  backend_memory  = "512"

  # Embedding service settings
  embedding_port   = var.embedding_port
  embedding_cpu    = "1024"
  embedding_memory = "2048"

  # Environment variables For Backend API
  backend_env_vars = {
    # ASP.NET Core
    "ASPNETCORE_ENVIRONMENT" = "Production"

    # Database — .env: DATABASE_CONNECTION_STRING
    "DATABASE_CONNECTION_STRING" = "Host=${module.rds.endpoint};Port=5432;Database=${module.rds.db_name};Username=${var.db_username};Password=${var.db_password};SSL Mode=Require;Trust Server Certificate=true"

    # Redis — .env: REDIS_HOST, REDIS_PORT, REDIS_PASSWORD, REDIS_INSTANCE_NAME
    "REDIS_HOST"          = module.ec2.private_ip
    "REDIS_PORT"          = "6379"
    "REDIS_PASSWORD"      = var.redis_pass
    "REDIS_INSTANCE_NAME" = "AIPromo"

    # RabbitMQ — .env: RABBITMQ_HOST, RABBITMQ_PORT, RABBITMQ_USER, RABBITMQ_PASS
    "RABBITMQ_HOST" = module.ec2.private_ip
    "RABBITMQ_PORT" = "5672"
    "RABBITMQ_USER" = var.rabbitmq_user
    "RABBITMQ_PASS" = var.rabbitmq_pass

    # Qdrant — .env: QDRANT_HOST, QDRANT_PORT, QDRANT_USE_HTTPS, QDRANT_API_KEY
    "QDRANT_HOST"      = module.ec2.private_ip
    "QDRANT_PORT"      = "6334"
    "QDRANT_USE_HTTPS" = "false"
    "QDRANT_API_KEY"   = ""

    # MinIO (S3) — .env: MINIO_ENDPOINT, MINIO_ACCESS_KEY, MINIO_SECRET_KEY, MINIO_USE_SSL
    "MINIO_ENDPOINT"   = "${module.s3.bucket_name}.s3.${var.aws_region}.amazonaws.com"
    "MINIO_ACCESS_KEY" = module.s3.iam_access_key_id
    "MINIO_SECRET_KEY" = module.s3.iam_secret_access_key
    "MINIO_USE_SSL"    = "true"

    # JWT — .env: JWT_SECRET, JWT_ISSUER, JWT_AUDIENCE, JWT_EXPIRY_MINUTES, JWT_REFRESH_TOKEN_EXPIRY_DAYS
    "JWT_SECRET"                    = var.jwt_secret
    "JWT_ISSUER"                    = "aipromo"
    "JWT_AUDIENCE"                  = "aipromo-api"
    "JWT_EXPIRY_MINUTES"            = "60"
    "JWT_REFRESH_TOKEN_EXPIRY_DAYS" = "7"

    # Embedding Service — .env: EMBEDDING_BASE_URL, EMBEDDING_TIMEOUT_SECONDS
    "EMBEDDING_BASE_URL"        = "http://${module.ec2.private_ip}:${var.embedding_port}"
    "EMBEDDING_TIMEOUT_SECONDS" = "30"

    # Embedding Queue — .env: EMBEDDING_REQUEST_QUEUE, EMBEDDING_RESPONSE_QUEUE
    "EMBEDDING_REQUEST_QUEUE"  = "embedding.requests"
    "EMBEDDING_RESPONSE_QUEUE" = "embedding.responses"

    # AI Model APIs
    "GEMINI_API_KEY"     = ""
    "GEMINI_MODEL"       = "gemini-2.5-flash"
    "OPENROUTER_API_KEY" = ""
    "OPENROUTER_MODEL"   = "black-forest-labs/flux.2-pro"

    # Logging
    "LOG_LEVEL_DEFAULT"   = "Information"
    "LOG_LEVEL_MICROSOFT" = "Warning"
    "LOG_LEVEL_EF"        = "Warning"
    "LOG_LEVEL_QUARTZ"    = "Warning"
  }

  # Embedding service env vars
  embedding_env_vars = {
    "HTTP_PORT"      = "8001"
    "RABBITMQ_HOST"  = module.ec2.private_ip
    "RABBITMQ_PORT"  = "5672"
    "RABBITMQ_USER"  = var.rabbitmq_user
    "RABBITMQ_PASS"  = var.rabbitmq_pass
    "REQUEST_QUEUE"  = "embedding.requests"
    "RESPONSE_QUEUE" = "embedding.responses"
    "MODEL_NAME"     = "bge-small-en-v1.5"
  }
}

# ============================================
# EC2 — ECS Container Host + Nginx + Certbot
# ============================================

module "ec2" {
  source = "../../modules/ec2"

  project                     = var.project
  environment                 = var.environment
  vpc_id                      = module.vpc.vpc_id
  public_subnet_id            = module.vpc.public_subnet_ids[0]
  instance_type               = var.ec2_instance_type
  key_name                    = var.ec2_key_name
  allowed_cidr                = var.allowed_cidr
  ecs_instance_profile_name   = module.ecs.ecs_instance_profile_name
  domain_name                 = var.domain_name
  rabbitmq_user               = var.rabbitmq_user
  rabbitmq_pass               = var.rabbitmq_pass
  redis_pass                  = var.redis_pass
  backend_port                = var.backend_port
}

# ============================================
# RDS — PostgreSQL Database
# ============================================

module "rds" {
  source = "../../modules/rds"

  project                 = var.project
  environment             = var.environment
  vpc_id                  = module.vpc.vpc_id
  private_subnet_ids      = module.vpc.private_subnet_ids
  ec2_security_group_id = module.ec2.security_group_id
  db_name                 = var.db_name
  db_username             = var.db_username
  db_password             = var.db_password
  instance_class          = var.rds_instance_class
}

# ============================================
# Route 53 — DNS
# ============================================

module "route53" {
  source = "../../modules/route53"

  project         = var.project
  environment     = var.environment
  domain_name     = var.domain_name
  api_subdomain   = var.api_subdomain
  ec2_eip         = module.ec2.public_ip
}
