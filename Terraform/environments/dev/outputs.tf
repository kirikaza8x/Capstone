# ============================================
# Development Environment Outputs
# ============================================

output "ec2_public_ip" {
  value       = module.ec2.public_ip
  description = "EC2 Elastic IP — use for SSH and access API"
}

output "rds_endpoint" {
  value       = module.rds.endpoint
  description = "RDS PostgreSQL endpoint"
}

output "s3_bucket_name" {
  value       = module.s3.bucket_name
  description = "S3 bucket name for media storage"
}

output "ecs_cluster_name" {
  value       = module.ecs.cluster_name
  description = "ECS Cluster name"
}

output "api_url" {
  value       = "https://${module.route53.api_fqdn}"
  description = "API URL (HTTPS)"
}

output "api_fqdn" {
  value       = module.route53.api_fqdn
  description = "API fully qualified domain name"
}

output "ecr_backend_api_url" {
  value       = module.ecr.backend_api_repository_url
  description = "ECR repository URL for Backend API"
}

output "ecr_embedding_url" {
  value       = module.ecr.embedding_repository_url
  description = "ECR repository URL for Embedding service"
}

output "connection_info" {
  value = {
    postgres = "Host=${module.rds.endpoint};Database=${module.rds.db_name};Username=${var.db_username}"
    redis    = "${module.ec2.private_ip}:6379"
    rabbitmq = "amqp://${var.rabbitmq_user}:***@${module.ec2.private_ip}:5672"
    qdrant   = "http://${module.ec2.private_ip}:6333"
  }
  description = "Connection strings for appsettings.json"
  sensitive   = true
}
