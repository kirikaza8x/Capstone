# ============================================
# ECR Module Outputs
# ============================================

output "backend_api_repository_url" {
  value       = aws_ecr_repository.backend_api.repository_url
  description = "ECR repository URL for Backend API"
}

output "embedding_repository_url" {
  value       = aws_ecr_repository.embedding.repository_url
  description = "ECR repository URL for Embedding service"
}

output "backend_api_repository_arn" {
  value       = aws_ecr_repository.backend_api.arn
  description = "ECR repository ARN for Backend API"
}

output "embedding_repository_arn" {
  value       = aws_ecr_repository.embedding.arn
  description = "ECR repository ARN for Embedding service"
}
