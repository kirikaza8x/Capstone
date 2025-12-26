# Capstone - Microservices Architecture

## Technology Stack

### Backend Services
**Runtime & Framework:** .NET 9.0, ASP.NET Core

**Architecture:** Microservices (ConfigsDB Service, Users Service)  
**Design Patterns:** Clean Architecture, CQRS, Event-Driven Architecture, Saga Pattern

**API & Communication:**
- OpenAPI/Swagger
- MassTransit 8.5.7 (Service Bus)
- RabbitMQ (Message Broker)
- JWT Authentication

**Data Access & ORM:**
- Entity Framework Core 9.0.11
- PostgreSQL 15
- Redis 7 (Distributed Caching)

**Request Handling & Validation:**
- MediatR 12.2.0 (Mediator Pattern)
- FluentValidation 12.1.0
- AutoMapper (Object Mapping)

**Security & Utilities:**
- BCrypt.Net (Password Hashing)
- System.IdentityModel.Tokens.Jwt (JWT Tokens)
- Scrutor (Advanced Dependency Injection)
- EFCore.BulkExtensions (Batch Operations)

**Testing:** XUnit-based unit test projects per layer (API, Application, Domain, Infrastructure)

### Client-Side Technologies
**Web Client:** ReactJS, Redux Toolkit, HTML5, CSS3, React Query, TailwindCSS

### Infrastructure & DevOps
- Docker & Docker Compose
- PostgreSQL 15
- RabbitMQ (Message Queue)
- Redis 7-alpine (Cache)

---

## Setup & Development

### Quick Start
# 1. Stop containers and DELETE ALL VOLUMES (Critical for deep clean)
docker-compose down -v

# 2. Clear build cache (Optional, ensures code changes are picked up)
docker builder prune -a -f

# 3. Build and Start everything
docker-compose up --build --force-recreate