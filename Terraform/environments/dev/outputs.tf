output "ec2_public_ip" {
  value       = module.ec2.public_ip
  description = "EC2 public IP — dùng để connect RabbitMQ, Redis, Qdrant"
}

output "rds_endpoint" {
  value       = module.rds.endpoint
  description = "RDS PostgreSQL endpoint"
}

output "s3_bucket_name" {
  value       = module.s3.bucket_name
  description = "S3 bucket name"
}

output "s3_access_key_id" {
  value       = module.s3.iam_access_key_id
  description = "S3 IAM access key ID"
}

output "s3_secret_access_key" {
  value       = module.s3.iam_secret_access_key
  sensitive   = true
  description = "S3 IAM secret access key"
}

output "connection_strings" {
  value = {
    postgres  = "Host=${module.rds.endpoint};Database=${module.rds.db_name}"
    rabbitmq  = "amqp://aipromo:PASSWORD@${module.ec2.public_ip}:5672"
    redis     = "${module.ec2.public_ip}:6379"
    qdrant    = "http://${module.ec2.public_ip}:6333"
  }
  description = "Connection strings cho appsettings.json"
}