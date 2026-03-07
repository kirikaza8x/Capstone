# Stage 1: Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY Api/Api/*.csproj ./Api/Api/
RUN dotnet restore ./Api/Api/Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/Api/Api
RUN dotnet publish Api.csproj -c Release -o /app/publish

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Production Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]


