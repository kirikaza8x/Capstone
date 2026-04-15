# ============================================
# ECS Cluster
# ============================================

resource "aws_ecs_cluster" "main" {
  name = "${var.project}-${var.environment}-cluster"

  setting {
    name  = "containerInsights"
    value = "disabled"  # enable when production, disabled to save CloudWatch cost
  }

  tags = {
    Name        = "${var.project}-${var.environment}-cluster"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# IAM Role For ECS EC2 Instance (container host)
# ============================================

resource "aws_iam_role" "ecs_instance_role" {
  name = "${var.project}-${var.environment}-ecs-instance-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name        = "${var.project}-${var.environment}-ecs-instance-role"
    Project     = var.project
    Environment = var.environment
  }
}

resource "aws_iam_role_policy_attachment" "ecs_instance_role" {
  role       = aws_iam_role.ecs_instance_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonEC2ContainerServiceforEC2Role"
}

resource "aws_iam_instance_profile" "ecs_instance_profile" {
  name = "${var.project}-${var.environment}-ecs-instance-profile"
  role = aws_iam_role.ecs_instance_role.name
}

# ============================================
# IAM Role For ECS Task Execution (pull image, push logs)
# ============================================

resource "aws_iam_role" "ecs_task_execution_role" {
  name = "${var.project}-${var.environment}-ecs-task-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name        = "${var.project}-${var.environment}-ecs-task-execution-role"
    Project     = var.project
    Environment = var.environment
  }
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_role" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# ============================================
# IAM Role For ECS Task (app permissions: S3 access)
# ============================================

resource "aws_iam_role" "ecs_task_role" {
  name = "${var.project}-${var.environment}-ecs-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name        = "${var.project}-${var.environment}-ecs-task-role"
    Project     = var.project
    Environment = var.environment
  }
}

resource "aws_iam_role_policy" "ecs_task_s3" {
  name = "${var.project}-${var.environment}-ecs-task-s3-policy"
  role = aws_iam_role.ecs_task_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
          "s3:ListBucket"
        ]
        Resource = [
          var.s3_bucket_arn,
          "${var.s3_bucket_arn}/*"
        ]
      }
    ]
  })
}

# ============================================
# CloudWatch Log Group For ECS Tasks
# ============================================

resource "aws_cloudwatch_log_group" "backend_api" {
  name              = "/ecs/${var.project}-${var.environment}/backend-api"
  retention_in_days = 7 

  tags = {
    Name        = "${var.project}-${var.environment}-backend-api-logs"
    Project     = var.project
    Environment = var.environment
  }
}

resource "aws_cloudwatch_log_group" "embedding" {
  name              = "/ecs/${var.project}-${var.environment}/embedding"
  retention_in_days = 7

  tags = {
    Name        = "${var.project}-${var.environment}-embedding-logs"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# Security Group For ECS Tasks
# ============================================

resource "aws_security_group" "ecs_tasks" {
  name        = "${var.project}-${var.environment}-ecs-tasks-sg"
  description = "Security group for ECS tasks"
  vpc_id      = var.vpc_id

  # Enable traffic from EC2 host (Nginx reverse proxy)
  ingress {
    from_port       = var.backend_port
    to_port         = var.backend_port
    protocol        = "tcp"
    security_groups = [var.ec2_security_group_id]
  }

  # Embedding service port (internal only)
  ingress {
    from_port       = var.embedding_port
    to_port         = var.embedding_port
    protocol        = "tcp"
    security_groups = [var.ec2_security_group_id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project}-${var.environment}-ecs-tasks-sg"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# ECS Task Definition — Backend API
# ============================================

resource "aws_ecs_task_definition" "backend_api" {
  family                   = "${var.project}-${var.environment}-backend-api"
  network_mode             = "bridge"
  requires_compatibilities = ["EC2"]
  cpu                      = var.backend_cpu
  memory                   = var.backend_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name      = "backend-api"
      image     = "${var.ecr_repository_url}:latest"
      essential = true
      portMappings = [
        {
          containerPort = 8080  # .NET app listen port (từ EXPOSE trong Dockerfile)
          hostPort      = var.backend_port  # Port Nginx proxy tới (5000)
          protocol      = "tcp"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.backend_api.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "backend-api"
        }
      }
      environment = var.backend_env_vars != null ? [
        for key, value in var.backend_env_vars : {
          name  = key
          value = value
        }
      ] : []
      # Tạm thời không có health check vì backend chưa có /health endpoint
      # Sau này thêm endpoint thì bỏ comment dòng dưới
      # healthCheck = {
      #   command     = ["CMD-SHELL", "curl -f http://localhost:${var.backend_port}/health || exit 1"]
      #   interval    = 30
      #   timeout     = 5
      #   retries     = 3
      #   startPeriod = 60
      # }
    }
  ])

  tags = {
    Name        = "${var.project}-${var.environment}-backend-api"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# ECS Task Definition — Embedding Service
# ============================================

resource "aws_ecs_task_definition" "embedding" {
  family                   = "${var.project}-${var.environment}-embedding"
  network_mode             = "bridge"
  requires_compatibilities = ["EC2"]
  cpu                      = var.embedding_cpu
  memory                   = var.embedding_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name      = "embedding"
      image     = "${var.embedding_ecr_repository_url}:latest"
      essential = true
      portMappings = [
        {
          containerPort = 8001  # Embedding app listen port (từ EXPOSE + HTTP_PORT trong Dockerfile)
          hostPort      = var.embedding_port  # Port exposed on host (5100)
          protocol      = "tcp"
        }
      ]
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = aws_cloudwatch_log_group.embedding.name
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "embedding"
        }
      }
      environment = var.embedding_env_vars != null ? [
        for key, value in var.embedding_env_vars : {
          name  = key
          value = value
        }
      ] : []
      # Tạm thời tắt health check
      # healthCheck = {
      #   command     = ["CMD-SHELL", "curl -f http://localhost:${var.embedding_port}/health || exit 1"]
      #   interval    = 60
      #   timeout     = 10
      #   retries     = 3
      #   startPeriod = 120
      # }
    }
  ])

  tags = {
    Name        = "${var.project}-${var.environment}-embedding"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# ECS Service — Backend API
# ============================================

resource "aws_ecs_service" "backend_api" {
  name            = "${var.project}-${var.environment}-backend-api-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.backend_api.arn
  desired_count   = 1
  launch_type     = "EC2"

  # Bridge mode: stop old before start new (port conflict prevention)
  deployment_maximum_percent         = 100
  deployment_minimum_healthy_percent = 0

  # Circuit breaker: temporarily disabled to allow slow startups
  # deployment_circuit_breaker {
  #   enable   = true
  #   rollback = true
  # }

  tags = {
    Name        = "${var.project}-${var.environment}-backend-api-service"
    Project     = var.project
    Environment = var.environment
  }
}

# ============================================
# ECS Service — Embedding (Always on for API dependency)
# ============================================

resource "aws_ecs_service" "embedding" {
  name            = "${var.project}-${var.environment}-embedding-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.embedding.arn
  desired_count   = 1  # Always on
  launch_type     = "EC2"

  deployment_maximum_percent         = 200
  deployment_minimum_healthy_percent = 0

  # Circuit breaker: temporarily disabled
  # deployment_circuit_breaker {
  #   enable   = true
  #   rollback = true
  # }

  tags = {
    Name        = "${var.project}-${var.environment}-embedding-service"
    Project     = var.project
    Environment = var.environment
  }
}
