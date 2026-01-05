param(
    [string]$ProjectName,
    [string]$ServiceName
)

if (-not $ProjectName -or -not $ServiceName) {
    Write-Host "`n[X] Missing parameters." -ForegroundColor Red
    Write-Host "Usage: ./create_clean_arch.ps1 <ProjectName> <ServiceName>" -ForegroundColor Yellow
    exit 1
}

# Paths
$ScriptDir     = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir    = Join-Path $ScriptDir "..\..\Backend\$ProjectName"
$MicroRoot     = Join-Path $BackendDir "$ServiceName.Microservice"
$SrcDir        = Join-Path $MicroRoot "src"
$TestDir       = Join-Path $MicroRoot "tests"
$SolutionPath  = Join-Path $BackendDir "$ProjectName.sln"
$ApiDir        = Join-Path $SrcDir "Api"
$Dockerfile    = Join-Path $ApiDir "Dockerfile"

# === SAFEGUARDS ===
$existingProjects = Get-ChildItem -Path $SrcDir -Recurse -Filter *.csproj -ErrorAction SilentlyContinue
if ($existingProjects) {
    Write-Host "`n[X] Existing project files detected under $SrcDir. Aborting to avoid overwriting." -ForegroundColor Red
    exit 1
}
if (Test-Path $Dockerfile) {
    Write-Host "`n[X] Dockerfile already exists at $Dockerfile. Aborting to avoid overwriting." -ForegroundColor Red
    exit 1
}

# === Create folders ===
New-Item -ItemType Directory -Force -Path "$SrcDir/Infrastructure","$SrcDir/Api","$SrcDir/Application","$SrcDir/Domain" | Out-Null
New-Item -ItemType Directory -Force -Path "$TestDir/Infrastructure","$TestDir/Api","$TestDir/Application","$TestDir/Domain" | Out-Null

# Subfolders
New-Item -ItemType Directory -Force -Path "$ApiDir/Controllers" | Out-Null
New-Item -ItemType Directory -Force -Path "$SrcDir/Application/Features","$SrcDir/Application/Mappings" | Out-Null
New-Item -ItemType Directory -Force -Path "$SrcDir/Domain/Entities","$SrcDir/Domain/Enums","$SrcDir/Domain/Events","$SrcDir/Domain/Repositories","$SrcDir/Domain/UOW" | Out-Null
New-Item -ItemType Directory -Force -Path "$SrcDir/Infrastructure/Persistence/Configs","$SrcDir/Infrastructure/Persistence/Contexts","$SrcDir/Infrastructure/Repositories","$SrcDir/Infrastructure/UOW" | Out-Null

# === Create projects ===
dotnet new classlib -n "Infrastructure" -o "$SrcDir/Infrastructure"
dotnet new classlib -n "Application"   -o "$SrcDir/Application"
dotnet new classlib -n "Domain"        -o "$SrcDir/Domain"
dotnet new webapi   -n "Api"           -o "$ApiDir"

dotnet new xunit -n "Infrastructure.Tests" -o "$TestDir/Infrastructure"
dotnet new xunit -n "Application.Tests"    -o "$TestDir/Application"
dotnet new xunit -n "Domain.Tests"         -o "$TestDir/Domain"
dotnet new xunit -n "Api.Tests"            -o "$TestDir/Api"

# === Add references ===
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" reference "$SrcDir/Application/Application.csproj"
dotnet add "$SrcDir/Application/Application.csproj" reference "$SrcDir/Domain/Domain.csproj"
dotnet add "$ApiDir/Api.csproj" reference "$SrcDir/Infrastructure/Infrastructure.csproj"

dotnet add "$TestDir/Infrastructure/Infrastructure.Tests.csproj" reference "$SrcDir/Infrastructure/Infrastructure.csproj"
dotnet add "$TestDir/Application/Application.Tests.csproj" reference "$SrcDir/Application/Application.csproj"
dotnet add "$TestDir/Domain/Domain.Tests.csproj" reference "$SrcDir/Domain/Domain.csproj"
dotnet add "$TestDir/Api/Api.Tests.csproj" reference "$ApiDir/Api.csproj"

# === Solution ===
if (-not (Test-Path $SolutionPath)) {
    dotnet new sln -n "$ProjectName" -o $BackendDir
}
dotnet sln "$SolutionPath" add "$ApiDir/Api.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Application/Application.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Domain/Domain.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Infrastructure/Infrastructure.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Api/Api.Tests.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Application/Application.Tests.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Domain/Domain.Tests.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Infrastructure/Infrastructure.Tests.csproj"

# === Marker AssemblyReference + DI classes ===
@"
namespace $ServiceName.Api
{
    public static class PresentationAssemblyReference { }
    //public static class DependencyInjection
    //{
    //    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    //    {
    //        services.AddControllers();
    //        services.AddSwaggerGen();
    //        return services;
    //    }
    //}
}
"@ | Set-Content "$ApiDir/AssemblyReference.cs"

@"
namespace $ServiceName.Application
{
    public static class ApplicationAssemblyReference { }
    //public static class DependencyInjection
    //{
    //    public static IServiceCollection AddApplication(this IServiceCollection services)
    //    {
    //        return services;
    //    }
    //}
}
"@ | Set-Content "$SrcDir/Application/AssemblyReference.cs"

@"
namespace $ServiceName.Domain
{
    public static class DomainAssemblyReference { }
    //public static class DependencyInjection
    //{
    //    public static IServiceCollection AddDomain(this IServiceCollection services)
    //    {
    //        return services;
    //    }
    //}
}
"@ | Set-Content "$SrcDir/Domain/AssemblyReference.cs"

@"
namespace $ServiceName.Infrastructure
{
    public static class InfrastructureAssemblyReference { }
    //public static class DependencyInjection
    //{
    //    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    //    {
    //        return services;
    //    }
    //}
}
"@ | Set-Content "$SrcDir/Infrastructure/AssemblyReference.cs"

# === Dockerfile ===
@"
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.sln .
COPY src/Api/Api.csproj src/Api/
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
RUN dotnet restore src/Api/Api.csproj
COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
"@ | Set-Content $Dockerfile

Write-Host "`n[OK] Clean Architecture scaffold created with DI and AssemblyReference for '$ServiceName'." -ForegroundColor Green
