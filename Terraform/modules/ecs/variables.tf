# ============================================
# ECS Module Variables
# ============================================

variable "project" {
  description = "Project name"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "vpc_id" {
  description = "VPC ID"
  type        = string
}

variable "ec2_security_group_id" {
  description = "Security group ID of the EC2 container host (for Nginx → ECS traffic)"
  type        = string
}

variable "ecr_repository_url" {
  description = "ECR repository URL for Backend API image"
  type        = string
}

variable "embedding_ecr_repository_url" {
  description = "ECR repository URL for Embedding service image"
  type        = string
}

variable "s3_bucket_arn" {
  description = "S3 bucket ARN for media storage"
  type        = string
}

# --- Backend API container settings ---

variable "backend_port" {
  description = "Port that Backend API listens on"
  type        = number
  default     = 5000
}

variable "backend_cpu" {
  description = "CPU units for Backend API container (256 = 0.25 vCPU, 512 = 0.5 vCPU)"
  type        = string
  default     = "512"
}

variable "backend_memory" {
  description = "Memory in MB for Backend API container"
  type        = string
  default     = "512"
}

variable "backend_env_vars" {
  description = "Environment variables for Backend API container"
  type        = map(string)
  default     = {}
}

# --- Embedding Service container settings ---

variable "embedding_port" {
  description = "Port that Embedding service listens on"
  type        = number
  default     = 5100
}

variable "embedding_cpu" {
  description = "CPU units for Embedding container (1024 = 1 vCPU)"
  type        = string
  default     = "1024"
}

variable "embedding_memory" {
  description = "Memory in MB for Embedding container"
  type        = string
  default     = "2048"
}

variable "embedding_env_vars" {
  description = "Environment variables for Embedding container"
  type        = map(string)
  default     = {}
}
