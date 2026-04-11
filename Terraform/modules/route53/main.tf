# ============================================
# Route 53 — DNS
# ============================================

# Lookup existing hosted zone (do not create new — zone already exists from domain purchase)
data "aws_route53_zone" "main" {
  name         = var.domain_name
  private_zone = false
}

# A record pointing to domain → EC2 EIP
resource "aws_route53_record" "api" {
  zone_id = data.aws_route53_zone.main.zone_id
  name    = var.api_subdomain != "" ? "${var.api_subdomain}.${var.domain_name}" : var.domain_name
  type    = "A"
  ttl     = 300
  records = [var.ec2_eip]
}
