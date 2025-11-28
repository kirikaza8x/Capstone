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

# Create folders
New-Item -ItemType Directory -Force -Path "$SrcDir/Infrastructure","$SrcDir/Api","$SrcDir/Application","$SrcDir/Domain" | Out-Null
New-Item -ItemType Directory -Force -Path "$TestDir/Infrastructure","$TestDir/Api","$TestDir/Application","$TestDir/Domain" | Out-Null

# Create projects in src
Write-Host "[>] Creating source projects..." -ForegroundColor Cyan
dotnet new classlib -n "Infrastructure" -o "$SrcDir/Infrastructure"
dotnet new classlib -n "Application"   -o "$SrcDir/Application"
dotnet new classlib -n "Domain"        -o "$SrcDir/Domain"
dotnet new webapi   -n "Api"           -o "$ApiDir"

# Create test projects
Write-Host "[>] Creating test projects..." -ForegroundColor Cyan
dotnet new xunit -n "Infrastructure.Tests" -o "$TestDir/Infrastructure"
Write-Host "[OK] Infrastructure.Tests created" -ForegroundColor Green
dotnet new xunit -n "Application.Tests"    -o "$TestDir/Application"
Write-Host "[OK] Application.Tests created" -ForegroundColor Green
dotnet new xunit -n "Domain.Tests"         -o "$TestDir/Domain"
Write-Host "[OK] Domain.Tests created" -ForegroundColor Green
dotnet new xunit -n "Api.Tests"            -o "$TestDir/Api"
Write-Host "[OK] Api.Tests created" -ForegroundColor Green

# Add references between src projects
Write-Host "[>] Adding references between source projects..." -ForegroundColor Cyan
dotnet add "$SrcDir/Infrastructure/Infrastructure.csproj" reference "$SrcDir/Application/Application.csproj"
dotnet add "$SrcDir/Application/Application.csproj" reference "$SrcDir/Domain/Domain.csproj"
dotnet add "$ApiDir/Api.csproj" reference "$SrcDir/Infrastructure/Infrastructure.csproj"

# Add references from test projects to src projects
Write-Host "[>] Adding references from test projects..." -ForegroundColor Cyan
dotnet add "$TestDir/Infrastructure/Infrastructure.Tests.csproj" reference "$SrcDir/Infrastructure/Infrastructure.csproj"
dotnet add "$TestDir/Application/Application.Tests.csproj" reference "$SrcDir/Application/Application.csproj"
dotnet add "$TestDir/Domain/Domain.Tests.csproj" reference "$SrcDir/Domain/Domain.csproj"
dotnet add "$TestDir/Api/Api.Tests.csproj" reference "$ApiDir/Api.csproj"

# Create shared solution if it doesn't exist
if (-not (Test-Path $SolutionPath)) {
    Write-Host "[>] Creating solution $ProjectName.sln..." -ForegroundColor Cyan
    dotnet new sln -n "$ProjectName" -o $BackendDir
}

# Add all projects to shared solution
Write-Host "[>] Adding projects to solution..." -ForegroundColor Cyan
dotnet sln "$SolutionPath" add "$ApiDir/Api.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Application/Application.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Domain/Domain.csproj"
dotnet sln "$SolutionPath" add "$SrcDir/Infrastructure/Infrastructure.csproj"

dotnet sln "$SolutionPath" add "$TestDir/Api/Api.Tests.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Application/Application.Tests.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Domain/Domain.Tests.csproj"
dotnet sln "$SolutionPath" add "$TestDir/Infrastructure/Infrastructure.Tests.csproj"

# Create Dockerfile in Api layer (targeting .NET 9.0)
Write-Host "[>] Creating Dockerfile for Api project..." -ForegroundColor Cyan
@"
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and projects
COPY *.sln .
COPY src/Api/Api.csproj src/Api/
COPY src/Application/Application.csproj src/Application/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
RUN dotnet restore src/Api/Api.csproj

# Build and publish
COPY . .
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
"@ | Out-File -Encoding UTF8 $Dockerfile -Force

Write-Host "[OK] Dockerfile created at $Dockerfile" -ForegroundColor Green

# Final summary
Write-Host "`n[OK] Microservice '$ServiceName' created under project '$ProjectName'." -ForegroundColor Green
Write-Host "    [DIR]   $MicroRoot" -ForegroundColor Yellow
Write-Host "    [SLN]   $SolutionPath" -ForegroundColor Yellow
Write-Host "    [DOCKER] $Dockerfile" -ForegroundColor Yellow
Write-Host "`n[>] To begin working, navigate to:" -ForegroundColor Magenta
Write-Host "    cd $MicroRoot" -ForegroundColor White
Write-Host "`nHappy Coding!" -ForegroundColor Green