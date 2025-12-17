# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY Capstone/*.sln .

# Copy microservice projects (relative to Backend context)
COPY Capstone/Users.Microservice/src/Api/Api.csproj Capstone/Users.Microservice/src/Api/
COPY Capstone/Users.Microservice/src/Application/Application.csproj Capstone/Users.Microservice/src/Application/
COPY Capstone/Users.Microservice/src/Domain/Domain.csproj Capstone/Users.Microservice/src/Domain/
COPY Capstone/Users.Microservice/src/Infrastructure/Infrastructure.csproj Capstone/Users.Microservice/src/Infrastructure/

# Copy shared projects (relative to Backend context)
COPY Shared/src/Shared.Application/Shared.Application.csproj Shared/src/Shared.Application/
COPY Shared/src/Shared.Contract/Shared.Contract.csproj Shared/src/Shared.Contract/
COPY Shared/src/Shared.Domain/Shared.Domain.csproj Shared/src/Shared.Domain/
COPY Shared/src/Shared.Infrastructure/Shared.Infrastructure.csproj Shared/src/Shared.Infrastructure/
COPY Shared/src/Shared.Presentation/Shared.Presentation.csproj Shared/src/Shared.Presentation/

# Restore dependencies
RUN dotnet restore Capstone/Users.Microservice/src/Api/Api.csproj

# Copy all source code
COPY Capstone/Users.Microservice/ Capstone/Users.Microservice/
COPY Shared/src/Shared.Application/ Shared/src/Shared.Application/
COPY Shared/src/Shared.Contract/ Shared/src/Shared.Contract/
COPY Shared/src/Shared.Domain/ Shared/src/Shared.Domain/
COPY Shared/src/Shared.Infrastructure/ Shared/src/Shared.Infrastructure/
COPY Shared/src/Shared.Presentation/ Shared/src/Shared.Presentation/

# Build and publish
RUN dotnet publish Capstone/Users.Microservice/src/Api/Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
