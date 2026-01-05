param(
    [string]$ProjectName,
    [string]$ServiceName,
    [string]$ConnectionString,
    [string]$DbContextName = "ProgramsDbContext"
)

if (-not $ProjectName -or -not $ServiceName -or -not $ConnectionString) {
    Write-Host "`n[X] Missing parameters." -ForegroundColor Red
    Write-Host "Usage: ./ef_scaffold.ps1 <ProjectName> <ServiceName> <ConnectionString> [DbContextName]" -ForegroundColor Yellow
    exit 1
}

# Paths
$ScriptDir     = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir    = Join-Path $ScriptDir "..\..\Backend\$ProjectName"
$MicroRoot     = Join-Path $BackendDir "$ServiceName.Microservice"
$SrcDir        = Join-Path $MicroRoot "src"
$ApiProject    = Join-Path $SrcDir "Api/Api.csproj"
$InfraProject  = Join-Path $SrcDir "Infrastructure/Infrastructure.csproj"

if (-not (Test-Path $ApiProject)) {
    Write-Host "`n[X] Api project not found for service '$ServiceName'." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $InfraProject)) {
    Write-Host "`n[X] Infrastructure project not found for service '$ServiceName'." -ForegroundColor Red
    exit 1
}

Write-Host "[>] Scaffolding DbContext for service '$ServiceName'..." -ForegroundColor Cyan

# ---------------- Scaffold DbContext ----------------
dotnet ef dbcontext scaffold "$ConnectionString" Npgsql.EntityFrameworkCore.PostgreSQL `
    --project "$InfraProject" `
    --startup-project "$ApiProject" `
    --output-dir "Persistence/Scaffold" `
    --context "$DbContextName" `
    --force

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Scaffolding failed." -ForegroundColor Red
    exit 1
}

Write-Host "`n[OK] DbContext '$DbContextName' scaffolded successfully for service '$ServiceName'." -ForegroundColor Green
