# ============================================
# ECS Module Outputs
# ============================================

output "cluster_id" {
  value       = aws_ecs_cluster.main.id
  description = "ECS Cluster ID"
}

output "cluster_name" {
  value       = aws_ecs_cluster.main.name
  description = "ECS Cluster Name"
}

output "ecs_instance_role_arn" {
  value       = aws_iam_role.ecs_instance_role.arn
  description = "IAM Role ARN for ECS EC2 instance profile"
}

output "ecs_instance_profile_name" {
  value       = aws_iam_instance_profile.ecs_instance_profile.name
  description = "IAM Instance Profile name for ECS EC2 instance"
}

output "ecs_task_execution_role_arn" {
  value       = aws_iam_role.ecs_task_execution_role.arn
  description = "IAM Role ARN for ECS task execution"
}

output "ecs_task_role_arn" {
  value       = aws_iam_role.ecs_task_role.arn
  description = "IAM Role ARN for ECS task (app permissions)"
}

output "ecs_tasks_security_group_id" {
  value       = aws_security_group.ecs_tasks.id
  description = "Security Group ID for ECS tasks"
}

output "backend_api_log_group" {
  value       = aws_cloudwatch_log_group.backend_api.name
  description = "CloudWatch Log Group for Backend API"
}

output "embedding_log_group" {
  value       = aws_cloudwatch_log_group.embedding.name
  description = "CloudWatch Log Group for Embedding service"
}

output "backend_api_service_name" {
  value       = aws_ecs_service.backend_api.name
  description = "ECS Service name for Backend API"
}

output "embedding_service_name" {
  value       = aws_ecs_service.embedding.name
  description = "ECS Service name for Embedding"
}
