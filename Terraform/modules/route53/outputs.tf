# ============================================
# Route 53 Module Outputs
# ============================================

output "hosted_zone_id" {
  value       = aws_route53_zone.main.zone_id
  description = "Route 53 Hosted Zone ID"
}

output "api_fqdn" {
  value       = aws_route53_record.api.fqdn
  description = "Fully qualified domain name for API"
}

output "api_record_name" {
  value       = aws_route53_record.api.name
  description = "API record name"
}

output "name_servers" {
  value       = aws_route53_zone.main.name_servers
  description = "4 Nameservers need to update on Hostinger"
}