# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Capstone/*.sln .

COPY Capstone/ConfigsDB.Microservice/src/Api/Api.csproj Capstone/ConfigsDB.Microservice/src/Api/
COPY Capstone/ConfigsDB.Microservice/src/Application/Application.csproj Capstone/ConfigsDB.Microservice/src/Application/
COPY Capstone/ConfigsDB.Microservice/src/Domain/Domain.csproj Capstone/ConfigsDB.Microservice/src/Domain/
COPY Capstone/ConfigsDB.Microservice/src/Infrastructure/Infrastructure.csproj Capstone/ConfigsDB.Microservice/src/Infrastructure/

COPY Shared/src/Shared.Application/Shared.Application.csproj Shared/src/Shared.Application/
COPY Shared/src/Shared.Contract/Shared.Contract.csproj Shared/src/Shared.Contract/
COPY Shared/src/Shared.Domain/Shared.Domain.csproj Shared/src/Shared.Domain/
COPY Shared/src/Shared.Infrastructure/Shared.Infrastructure.csproj Shared/src/Shared.Infrastructure/
COPY Shared/src/Shared.Presentation/Shared.Presentation.csproj Shared/src/Shared.Presentation/

RUN dotnet restore Capstone/ConfigsDB.Microservice/src/Api/Api.csproj

COPY Capstone/ConfigsDB.Microservice/ Capstone/ConfigsDB.Microservice/
COPY Shared/src/Shared.Application/ Shared/src/Shared.Application/
COPY Shared/src/Shared.Contract/ Shared/src/Shared.Contract/
COPY Shared/src/Shared.Domain/ Shared/src/Shared.Domain/
COPY Shared/src/Shared.Infrastructure/ Shared/src/Shared.Infrastructure/
COPY Shared/src/Shared.Presentation/ Shared/src/Shared.Presentation/

RUN dotnet publish Capstone/ConfigsDB.Microservice/src/Api/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
