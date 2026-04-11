# ============================================
# Route 53 Module Variables
# ============================================

variable "project" {
  description = "Project name"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "domain_name" {
  description = "Root domain name (e.g., aipromo.com)"
  type        = string
}

variable "api_subdomain" {
  description = "Subdomain for API (e.g., api). Leave empty for root domain."
  type        = string
  default     = "api"
}

variable "ec2_eip" {
  description = "EC2 Elastic IP address"
  type        = string
}
