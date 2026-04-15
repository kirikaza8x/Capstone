# AIPromo Backend – Event Management & AI Marketing System
**Project Code:** SP26SE115  
**Architecture:** Modular Monolith

# 1. Project Overview

**AIPromo** is a centralized Event Management & AI-driven Marketing platform.

### Goal
Provide organizers with:
- Event creation & management
- Digital ticketing system (QR-based)
- AI-powered marketing support
- Wallet & payment integration

# 2. Technical Stack

| Layer | Technology |
|-------|------------|
| Framework | ASP.NET Core (.NET 9) |
| Database | PostgreSQL 16 |
| CI/CD | GitHub Actions |
| Architecture | Modular Monolith |
| Messaging | RabbitMQ, MassTransit |
| Cache | Redis |
| Vector DB | Qdrant |
| Cloud | AWS (ECS, ECR, RDS, S3, Route53) |
| IaC | Terraform |

# 3. System Architecture

## 3.1 Business Modules

| Module | Purpose |
|--------|---------|
| **Users** | User management, authentication, OAuth (Google, Facebook) |
| **Events** | Event creation & management |
| **Ticketing** | QR-based digital ticketing, check-in |
| **Payment** | Payment processing (VNPay integration), wallet |
| **AI** | AI-powered marketing content generation (Gemini, OpenRouter) |
| **Notifications** | Email/SMS notifications (SMTP) |
| **Reports** | Analytics and reporting |

## 3.2 Additional Services

| Service | Technology | Purpose |
|---------|------------|---------|
| **Embedding Generator** | Python (FastAPI + ONNX) | Text embeddings using `bge-small-en-v1.5` model |
| **n8n** | Workflow Automation | Automated social media posting (Facebook) |

# 4. Project Setup Guide

## 4.1 Local Development

### Prerequisites
- .NET 9 SDK
- Docker Desktop
- Visual Studio 2022 / VS Code

### Steps

1. **Clone the repository**
```bash
git clone -b dev https://github.com/kirikaza8x/Capstone.git
cd Capstone
```

2. **Start infrastructure services**
```bash
docker-compose up -d
```

This command will start:
- PostgreSQL (Port 5432)
- Redis (Port 6379)
- RabbitMQ (Port 5672, Management: 15672)
- MinIO (Port 9000, Console: 9001)
- Qdrant (Port 6333)
- n8n (Port 5678)

Verify containers:
```bash
docker ps
```

3. **Configure `appsettings.json` in `Api` project**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Quartz": "Warning"
    }
  },
  "AllowedHosts": "*",

  "Outbox": {
    "IntervalInSeconds": 5,
    "BatchSize": 30,
    "MaxRetryCount": 3
  },
  "Inbox": {
    "IntervalInSeconds": 5,
    "BatchSize": 20,
    "RetentionDays": 7,
    "MaxRetryCount": 3
  },

  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=AIPromo;Username=postgres;Password=;SSL Mode=Disable;Trust Server Certificate=true",
    "MaxRetryCount": 3,
    "CommandTimeout": 30,
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": false
  },

  "Redis": {
    "Host": "localhost",
    "Port": 6379,
    "Password": "",
    "InstanceName": "AIPromo"
  },

  "MessageBroker": {
    "Host": "amqp://localhost:5672",
    "UserName": "aipromo",
    "Password": ""
  },

  "JwtConfigs": {
    "Secret": "your-secret-key",
    "Issuer": "test",
    "Audience": "test",
    "ExpiryMinutes": 100,
    "RefreshTokenExpiryDays": 7
  },

  "Storage": {
    "Endpoint": "http://localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "aipromo",
    "Region": "us-east-1",
    "UseSSL": false,
    "PublicUrl": "http://localhost:9000/aipromo"
  },

  "Authentication": {
    "Google": {
      "ServerClientId": "your-google-client-id"
    }
  },

  "Sms": {
    "Gmail": {
      "SmtpServer": "smtp.gmail.com",
      "SmtpPort": 587,
      "SenderEmail": "your-email@gmail.com",
      "SenderPassword": "your-app-password"
    }
  },

  "Embedding": {
    "Queue": {
      "RequestQueue": "embedding.requests",
      "ResponseQueue": "embedding.responses",
      "TimeoutSeconds": 30
    },
    "Http": {
      "BaseUrl": "http://localhost:8000/",
      "TimeoutSeconds": 30
    }
  },

  "Qdrant": {
    "Host": "localhost",
    "Port": 6334,
    "UseHttps": false,
    "ApiKey": "",
    "Collections": {
      "Events": {
        "Name": "event_embeddings",
        "VectorSize": 384
      },
      "UserBehavior": {
        "Name": "user_behavior_embeddings",
        "VectorSize": 384
      }
    }
  },

  "VNPay": {
    "Url": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TmnCode": "your-tmn-code",
    "HashSecret": "your-hash-secret",
    "ReturnUrl": "http://localhost:5173/payment/vnpay-return"
  },

  "Cors": {
    "AllowAnyOrigin": false,
    "AllowedOrigins": [
      "http://localhost:5500",
      "http://127.0.0.1:5500",
      "http://127.0.0.1:5000",
      "http://localhost:5000"
    ]
  },

  "N8nIntegration": {
    "DistributionWebhookUrl": "http://localhost:5678/webhook-test/post-distribute",
    "WebhookApiKey": "your-n8n-webhook-key",
    "AppBaseUrl": "https://localhost:7000/"
  },

  "Facebook": {
    "PageAccessToken": "your-facebook-page-token",
    "GraphApiVersion": "v23.0",
    "GraphApiBaseUrl": "https://graph.facebook.com/",
    "PageId": "your-facebook-page-id"
  },

  "RateLimiting": {
    "GlobalPolicy": "Global",
    "Policies": {
      "Global": {
        "PermitLimit": 120,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "Auth": {
        "PermitLimit": 10,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "AiGenerate": {
        "PermitLimit": 10,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "Payment": {
        "PermitLimit": 20,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "Webhook": {
        "PermitLimit": 60,
        "WindowSeconds": 60,
        "QueueLimit": 0
      },
      "Order": {
        "PermitLimit": 15,
        "WindowSeconds": 60,
        "QueueLimit": 0
      }
    }
  }
}
```

4. **Run the project**
- Open solution in Visual Studio
- Set `Api` project as startup project
- Press `F5`

5. **Access API**
- Swagger: `https://localhost:7000/swagger/index.html`

## 4.2 Docker Development

Alternatively, run everything in Docker:
```bash
docker-compose up -d
```

Services will be available at:
- Backend API: `http://localhost:5000`
- Embedding Service: `http://localhost:5100`
- Swagger: `http://localhost:5000/swagger`
