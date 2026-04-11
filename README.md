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
| Database | PostgreSQL |
| CI/CD | GitHub Actions |
| Architecture | Modular Monolith |
| Messaging | RabbitMQ, MassTransit |
| Cache | Redis |
# 3. Project Setup Guide
  1. Clone the repository at branch dev
```bash
git clone -b dev https://github.com/kirikaza8x/Capstone.git
```
  2. Go to root directory which include **docker-compose.yml** files, run below command:
```bash
docker-compose up -d
```

  This command will start:
  
  - PostgreSQL (Port 5432)
  - Redis (Port 6379)
  - RabbitMQ (Port 5672)
  - MinIO (Port 9000)
    
  Verify containers:
  ```bash
  docker ps
  ```
  
  All containers must be in `Up` status.

 3. Create `appsettings.json`  `Api project` following the example below:
```env
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Quartz": "Warning"
    }
  },

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
    "ConnectionString": "Host=localhost; Port=5432; Database=AIPromo; Username=postgres; Password=; SSL Mode=Disable; Trust Server Certificate=true",
    "MaxRetryCount": 3,
    "CommandTimeout": 30,
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": false
  },

  "Redis": {
    "Host": "localhost",
    "Port": ,
    "Password": "",
    "InstanceName": ""
  },

  "MessageBroker": {
    "Host": "",
    "UserName": "",
    "Password": ""
  },

  "JwtConfigs": {
    "Secret": "",
    "Issuer": "",
    "Audience": "",
    "ExpiryMinutes": 100,
    "RefreshTokenExpiryDays": 7
  },

  "Storage": {
    "Endpoint": "http://localhost:9000",
    "AccessKey": "",
    "SecretKey": "",
    "BucketName": "",
    "Region": "",
    "UseSSL": false,
    "PublicUrl": ""
  },
  "AllowedHosts": "*"
}
```
  4. Run project

    1. Open project in Visual Studio
    2. Set `Api project` as startup project
    3. Press `F5`.
     
  6. Access API
  Swagger:
  
  ```
  https://localhost:7000/swagger/index.html
  ```
