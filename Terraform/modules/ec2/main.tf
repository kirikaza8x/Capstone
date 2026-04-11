# ============================================
# Amazon Linux 2023 AMI (ECS-optimized)
# ============================================

data "aws_ami" "amazon_linux" {
  most_recent = true
  owners      = ["137112412989"] # Amazon

  filter {
    name   = "name"
    values = ["al2023-ami-2023.*-x86_64"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

# ============================================
# Security Group cho EC2 (Nginx + SSH)
# ============================================

resource "aws_security_group" "ec2" {
  name        = "${var.project}-${var.environment}-ec2-sg"
  description = "Security group for EC2 — Nginx reverse proxy + SSH"
  vpc_id      = var.vpc_id

  # HTTP — cho Let's Encrypt validation + redirect to HTTPS
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS — public API access
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # SSH — Just allow from CIDR
  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = var.allowed_cidr
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project}-${var.environment}-ec2-sg"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# EC2 Instance — ECS Container Host + Nginx
# ============================================

resource "aws_instance" "services" {
  ami                         = data.aws_ami.amazon_linux.id
  instance_type               = var.instance_type
  subnet_id                   = var.public_subnet_id
  vpc_security_group_ids      = [aws_security_group.ec2.id]
  key_name                    = var.key_name
  iam_instance_profile        = var.ecs_instance_profile_name
  associate_public_ip_address = true

  user_data = templatefile("${path.module}/user_data.sh", {
    project       = var.project
    environment   = var.environment
    domain_name   = var.domain_name
    rabbitmq_user = var.rabbitmq_user
    rabbitmq_pass = var.rabbitmq_pass
    redis_pass    = var.redis_pass
    backend_port  = var.backend_port
  })

  root_block_device {
    volume_size = 30
    volume_type = "gp3"
    encrypted   = true
  }

  tags = {
    Name        = "${var.project}-${var.environment}-ecs-host"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# Elastic IP
# ============================================

resource "aws_eip" "services" {
  instance = aws_instance.services.id
  domain   = "vpc"

  tags = {
    Name        = "${var.project}-${var.environment}-eip"
    Project     = var.project
    Environment = var.environment
  }
}