# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Define a variable for the microservice name
ARG SERVICE_NAME=Users.Microservice

# Copy solution file
COPY Capstone/*.sln .

# Copy microservice projects (relative to Backend context)
COPY Capstone/${SERVICE_NAME}/src/Api/Api.csproj Capstone/${SERVICE_NAME}/src/Api/
COPY Capstone/${SERVICE_NAME}/src/Application/Application.csproj Capstone/${SERVICE_NAME}/src/Application/
COPY Capstone/${SERVICE_NAME}/src/Domain/Domain.csproj Capstone/${SERVICE_NAME}/src/Domain/
COPY Capstone/${SERVICE_NAME}/src/Infrastructure/Infrastructure.csproj Capstone/${SERVICE_NAME}/src/Infrastructure/

# Copy shared projects (relative to Backend context)
COPY Shared/src/Shared.Application/Shared.Application.csproj Shared/src/Shared.Application/
COPY Shared/src/Shared.Contract/Shared.Contract.csproj Shared/src/Shared.Contract/
COPY Shared/src/Shared.Domain/Shared.Domain.csproj Shared/src/Shared.Domain/
COPY Shared/src/Shared.Infrastructure/Shared.Infrastructure.csproj Shared/src/Shared.Infrastructure/
COPY Shared/src/Shared.Presentation/Shared.Presentation.csproj Shared/src/Shared.Presentation/

# Restore dependencies
RUN dotnet restore Capstone/${SERVICE_NAME}/src/Api/Api.csproj

# Copy all source code
COPY Capstone/${SERVICE_NAME}/ Capstone/${SERVICE_NAME}/
COPY Shared/src/Shared.Application/ Shared/src/Shared.Application/
COPY Shared/src/Shared.Contract/ Shared/src/Shared.Contract/
COPY Shared/src/Shared.Domain/ Shared/src/Shared.Domain/
COPY Shared/src/Shared.Infrastructure/ Shared/src/Shared.Infrastructure/
COPY Shared/src/Shared.Presentation/ Shared/src/Shared.Presentation/

# Build and publish
RUN dotnet publish Capstone/${SERVICE_NAME}/src/Api/Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Force ASP.NET Core to listen on port 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Api.dll"]
