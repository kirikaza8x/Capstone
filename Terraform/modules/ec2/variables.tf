# ============================================
# EC2 Module Variables
# ============================================

variable "project" {
  description = "Project name"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "vpc_id" {
  description = "VPC ID"
  type        = string
}

variable "public_subnet_id" {
  description = "Public subnet ID for the EC2 instance"
  type        = string
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t3.medium"
}

variable "key_name" {
  description = "EC2 key pair name for SSH access"
  type        = string
}

variable "allowed_cidr" {
  description = "CIDR blocks allowed to SSH into EC2 (team IPs)"
  type        = list(string)
}

variable "ecs_instance_profile_name" {
  description = "IAM Instance Profile name for ECS Agent on EC2"
  type        = string
}

variable "domain_name" {
  description = "Domain name for SSL certificate (e.g., api.aipromo.com)"
  type        = string
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

variable "backend_port" {
  description = "Port that Backend API listens on (for Nginx upstream)"
  type        = number
  default     = 5000
}

variable "n8n_encryption_key" {
  description = "N8N encryption key"
  type        = string
  sensitive   = true
}

variable "db_host" {
  description = "RDS endpoint for n8n database connection"
  type        = string
}

variable "db_name" {
  description = "Database name"
  type        = string
}

variable "db_username" {
  description = "Database username"
  type        = string
}

variable "db_password" {
  description = "Database password"
  type        = string
  sensitive   = true
}