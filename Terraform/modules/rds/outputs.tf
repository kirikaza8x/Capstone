# ============================================
# RDS Module Outputs
# ============================================

output "endpoint" {
  value       = aws_db_instance.main.endpoint
  description = "RDS PostgreSQL endpoint (host:port)"
}

output "db_name" {
  value       = aws_db_instance.main.db_name
  description = "RDS database name"
}

output "security_group_id" {
  value       = aws_security_group.rds.id
  description = "RDS Security Group ID"
}
