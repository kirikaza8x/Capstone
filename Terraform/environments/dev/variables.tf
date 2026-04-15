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

# ─────────────────────────────────────────────────────────────
# Google OAuth Configuration
# ─────────────────────────────────────────────────────────────
variable "google_client_id" {
  description = "Google OAuth Client ID for server-side validation"
  type        = string
  sensitive   = true
  default     = ""
}

# ─────────────────────────────────────────────────────────────
# SMS / Email (Gmail SMTP) Configuration
# ─────────────────────────────────────────────────────────────
variable "smtp_server" {
  description = "SMTP server for sending emails"
  type        = string
  default     = "smtp.gmail.com"
}

variable "smtp_port" {
  description = "SMTP server port"
  type        = number
  default     = 587
}

variable "sender_email" {
  description = "Email address used to send notifications"
  type        = string
  sensitive   = true
  default     = ""
}

variable "sender_password" {
  description = "App password for sender email (Gmail App Password)"
  type        = string
  sensitive   = true
  default     = ""
}

# ─────────────────────────────────────────────────────────────
# VNPay Payment Gateway Configuration
# ─────────────────────────────────────────────────────────────
variable "vnpay_url" {
  description = "VNPay payment gateway URL"
  type        = string
  default     = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
}

variable "vnpay_tmn_code" {
  description = "VNPay Terminal Code (TmnCode) from VNPay merchant account"
  type        = string
  sensitive   = true
  default     = ""
}

variable "vnpay_hash_secret" {
  description = "VNPay Hash Secret for signing requests"
  type        = string
  sensitive   = true
  default     = ""
}

variable "vnpay_return_url" {
  description = "VNPay return URL after payment completion"
  type        = string
  default     = "https://aipromo.online/payment/return"
}

# ─────────────────────────────────────────────────────────────
# N8n & Facebook Integration Configuration
# ─────────────────────────────────────────────────────────────
variable "n8n_webhook_api_key" {
  description = "API Key for validating n8n webhook callbacks"
  type        = string
  sensitive   = true
  default     = ""
}

variable "n8n_distribution_webhook_url" {
  description = "n8n webhook URL for post distribution (e.g., https://n8n.domain.com/webhook/post-distribute)"
  type        = string
  default     = ""
}

variable "facebook_page_access_token" {
  description = "Facebook Page Access Token (long-lived) for posting and fetching metrics"
  type        = string
  sensitive   = true
  default     = ""
}

variable "facebook_page_id" {
  description = "Facebook Page ID"
  type        = string
  default     = ""
}

# ─────────────────────────────────────────────────────────────
# CORS Configuration
# ─────────────────────────────────────────────────────────────
variable "cors_allowed_origins" {
  description = "List of allowed origins for CORS (comma-separated for env var)"
  type        = list(string)
  default     = ["https://aipromo.online"]
}

variable "cors_allow_any_origin" {
  description = "Allow any origin (NOT recommended for production)"
  type        = bool
  default     = false
}

variable "gemini_api_key" {
  description = "API Key for Google Gemini"
  type        = string
  sensitive   = true
  default     = ""
}

variable "openrouter_api_key" {
  description = "API Key for OpenRouter"
  type        = string
  sensitive   = true
  default     = ""
}