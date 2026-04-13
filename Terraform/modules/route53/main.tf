# ============================================
# Route 53 — DNS
# ============================================

# Create new Hosted Zone for aipromo.online
resource "aws_route53_zone" "main" {
  name = var.domain_name

  tags = {
    Name        = var.domain_name
    Project     = var.project
    Environment = var.environment
  }
}

# A record pointing to domain → EC2 EIP
resource "aws_route53_record" "api" {
  zone_id = aws_route53_zone.main.zone_id
  name    = var.api_subdomain != "" ? "${var.api_subdomain}.${var.domain_name}" : var.domain_name
  type    = "A"
  ttl     = 300
  records = [var.ec2_eip]
}
