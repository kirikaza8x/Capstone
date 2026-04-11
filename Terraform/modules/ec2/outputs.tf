# ============================================
# EC2 Module Outputs
# ============================================

output "public_ip" {
  value       = aws_eip.services.public_ip
  description = "EC2 Elastic IP (public)"
}

output "private_ip" {
  value       = aws_instance.services.private_ip
  description = "EC2 private IP (for internal service communication)"
}

output "security_group_id" {
  value       = aws_security_group.ec2.id
  description = "EC2 Security Group ID"
}

output "instance_id" {
  value       = aws_instance.services.id
  description = "EC2 Instance ID"
}
