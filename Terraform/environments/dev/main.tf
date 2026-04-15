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
  backend_cpu     = "512"   # 0.5 vCPU (dev only)
  backend_memory  = "512"   # 512 MB

  # Embedding service settings
  embedding_port   = var.embedding_port
  embedding_cpu    = "512"  # 0.5 vCPU (dev only)
  embedding_memory = "1024" # 1 GB

  # Environment variables For Backend API
  backend_env_vars = {
    # ASP.NET Core
    "ASPNETCORE_ENVIRONMENT" = "Production"

    # Database — Section: "Database"
    "Database__ConnectionString" = "Host=${module.rds.endpoint};Database=${module.rds.db_name};Username=${var.db_username};Password=${var.db_password};SSL Mode=Require;Trust Server Certificate=true"
    "Database__MaxRetryCount"    = "3"
    "Database__CommandTimeout"   = "30"
    "Database__EnableDetailedErrors"       = "true"
    "Database__EnableSensitiveDataLogging" = "false"

    # Redis — Section: "Redis"
    "Redis__Host"         = module.ec2.private_ip
    "Redis__Port"         = "6379"
    "Redis__Password"     = var.redis_pass
    "Redis__InstanceName" = "AIPromo"

    # RabbitMQ — Section: "MessageBroker"
    "MessageBroker__Host"     = "rabbitmq://${module.ec2.private_ip}/"
    "MessageBroker__UserName" = var.rabbitmq_user
    "MessageBroker__Password" = var.rabbitmq_pass

    # Qdrant — Section: "Qdrant"
    "Qdrant__Host"      = module.ec2.private_ip
    "Qdrant__Port"      = "6334" #gRPC port
    "Qdrant__UseHttps"  = "false"
    "Qdrant__ApiKey"    = ""

    # Qdrant Collections — Section: "Qdrant:Collections"
    "Qdrant__Collections__Events__Name"        = "event_embeddings"
    "Qdrant__Collections__Events__VectorSize"  = "384"
    "Qdrant__Collections__UserBehavior__Name"        = "user_behavior_embeddings"
    "Qdrant__Collections__UserBehavior__VectorSize"  = "384"

    # Storage — Section: "Storage"
    "Storage__Endpoint"   = "s3.${var.aws_region}.amazonaws.com"
    "Storage__AccessKey"  = module.s3.iam_access_key_id
    "Storage__SecretKey"  = module.s3.iam_secret_access_key
    "Storage__BucketName" = module.s3.bucket_name
    "Storage__Region"     = var.aws_region
    "Storage__UseSSL"     = "true"

    # JWT — Section: "JwtConfigs"
    "JwtConfigs__Secret"                = var.jwt_secret
    "JwtConfigs__Issuer"                = "aipromo"
    "JwtConfigs__Audience"              = "aipromo-api"
    "JwtConfigs__ExpiryMinutes"         = "60"
    "JwtConfigs__RefreshTokenExpiryDays" = "7"

    # Gemini AI
    "Gemini__ApiKey" = var.gemini_api_key
    "Gemini__Model"  = "gemini-2.5-flash"

    # OpenRouter AI
    "OpenRouter__ApiKey" = var.openrouter_api_key
    "OpenRouter__Model"  = "black-forest-labs/flux.2-pro"

    # Embedding Service — Section: "Embedding"
    "Embedding__Http__BaseUrl"        = "http://${module.ec2.private_ip}:${var.embedding_port}"
    "Embedding__Http__TimeoutSeconds" = "30"

    # Embedding Queue — Section: "Embedding__Queue"
    "Embedding__Queue__RequestQueue"  = "embedding.requests"
    "Embedding__Queue__ResponseQueue" = "embedding.responses"
    "Embedding__Queue__TimeoutSeconds" = "30"

    # Google OAuth — Section: "Authentication__Google"
    "Authentication__Google__ServerClientId" = var.google_client_id

    # SMS / Email (Gmail SMTP) — Section: "Sms__Gmail"
    "Sms__Gmail__SmtpServer"     = var.smtp_server
    "Sms__Gmail__SmtpPort"       = tostring(var.smtp_port)
    "Sms__Gmail__SenderEmail"    = var.sender_email
    "Sms__Gmail__SenderPassword" = var.sender_password

    # VNPay Payment Gateway — Section: "VNPay"
    "VNPay__Url"         = var.vnpay_url
    "VNPay__TmnCode"     = var.vnpay_tmn_code
    "VNPay__HashSecret"  = var.vnpay_hash_secret
    "VNPay__ReturnUrl"   = var.vnpay_return_url

    # CORS — Section: "Cors"
    "Cors__AllowAnyOrigin" = var.cors_allow_any_origin ? "true" : "false"
    "Cors__AllowedOrigins__0" = var.cors_allowed_origins[0]
    "Cors__AllowedOrigins__1" = length(var.cors_allowed_origins) > 1 ? var.cors_allowed_origins[1] : ""
    "Cors__AllowedOrigins__2" = length(var.cors_allowed_origins) > 2 ? var.cors_allowed_origins[2] : ""
    "Cors__AllowedOrigins__3" = length(var.cors_allowed_origins) > 3 ? var.cors_allowed_origins[3] : ""
    "Cors__AllowedOrigins__4" = length(var.cors_allowed_origins) > 4 ? var.cors_allowed_origins[4] : ""
    "Cors__AllowedOrigins__5" = length(var.cors_allowed_origins) > 5 ? var.cors_allowed_origins[5] : ""

    # Logging
    "Logging__LogLevel__Default"                = "Information"
    "Logging__LogLevel__Microsoft.AspNetCore"   = "Warning"
    "Logging__LogLevel__Microsoft.EntityFrameworkCore" = "Warning"
    "Logging__LogLevel__Quartz"                 = "Warning"

    # Rate Limiting — Section: "RateLimiting"
    "RateLimiting__GlobalPolicy" = "Global"
    "RateLimiting__Policies__Global__PermitLimit" = "120"
    "RateLimiting__Policies__Global__WindowSeconds" = "60"
    "RateLimiting__Policies__Global__QueueLimit" = "0"
    "RateLimiting__Policies__Auth__PermitLimit" = "10"
    "RateLimiting__Policies__Auth__WindowSeconds" = "60"
    "RateLimiting__Policies__Auth__QueueLimit" = "0"
    "RateLimiting__Policies__AiGenerate__PermitLimit" = "10"
    "RateLimiting__Policies__AiGenerate__WindowSeconds" = "60"
    "RateLimiting__Policies__AiGenerate__QueueLimit" = "0"
    "RateLimiting__Policies__Payment__PermitLimit" = "20"
    "RateLimiting__Policies__Payment__WindowSeconds" = "60"
    "RateLimiting__Policies__Payment__QueueLimit" = "0"
    "RateLimiting__Policies__Webhook__PermitLimit" = "60"
    "RateLimiting__Policies__Webhook__WindowSeconds" = "60"
    "RateLimiting__Policies__Webhook__QueueLimit" = "0"
    "RateLimiting__Policies__Order__PermitLimit" = "15"
    "RateLimiting__Policies__Order__WindowSeconds" = "60"
    "RateLimiting__Policies__Order__QueueLimit" = "0"

    # N8n Integration — Section: "N8nIntegration"
    "N8nIntegration__DistributionWebhookUrl" = "https://n8n.yourdomain.com/webhook/post-distribute"  # URL n8n của bạn
    "N8nIntegration__WebhookApiKey"          = var.n8n_webhook_api_key
    "N8nIntegration__AppBaseUrl"             = "https://aipromo.online"

    # Facebook — Section: "Facebook"
    "Facebook__PageAccessToken"     = var.facebook_page_access_token
    "Facebook__PageId"              = var.facebook_page_id
    "Facebook__GraphApiVersion"     = "v23.0"
    "Facebook__GraphApiBaseUrl"     = "https://graph.facebook.com/"
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
  domain_name                 = "${var.api_subdomain}.${var.domain_name}" 
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
