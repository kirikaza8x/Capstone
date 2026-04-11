# ============================================
# Development Environment Variables
# ============================================

variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "project" {
  description = "Project name"
  type        = string
  default     = "aipromo"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "vpc_cidr" {
  description = "VPC CIDR block"
  type        = string
  default     = "10.0.0.0/16"
}

variable "ec2_instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t3.medium"
}

variable "ec2_key_name" {
  description = "EC2 key pair name for SSH access"
  type        = string
}

variable "allowed_cidr" {
  description = "CIDR blocks allowed to SSH into EC2 (team IPs)"
  type        = list(string)
  default     = ["0.0.0.0/0"] 
}

variable "db_name" {
  description = "RDS database name"
  type        = string
  default     = "aipromo"
}

variable "db_username" {
  description = "RDS master username"
  type        = string
  default     = "aipromo"
}

variable "db_password" {
  description = "RDS master password"
  type        = string
  sensitive   = true
}

variable "rds_instance_class" {
  description = "RDS instance class"
  type        = string
  default     = "db.t3.micro"
}

variable "domain_name" {
  description = "Root domain name"
  type        = string
}

variable "api_subdomain" {
  description = "Subdomain for API. Leave empty for root domain."
  type        = string
  default     = "api"
}

variable "backend_port" {
  description = "Port that Backend API listens on"
  type        = number
  default     = 5000
}

variable "embedding_port" {
  description = "Port that Embedding service listens on"
  type        = number
  default     = 5100
}

variable "rabbitmq_user" {
  description = "RabbitMQ default username"
  type        = string
  default     = "aipromo"
}

variable "rabbitmq_pass" {
  description = "RabbitMQ default password"
  type        = string
  sensitive   = true
}

variable "redis_pass" {
  description = "Redis password"
  type        = string
  sensitive   = true
}

variable "jwt_secret" {
  description = "JWT signing secret key"
  type        = string
  sensitive   = true
}
