param(
    [string]$ProjectName,
    [string]$ServiceName,
    [string]$MigrationName = "InitialCreate"
)

if (-not $ProjectName -or -not $ServiceName) {
    Write-Host "`n[X] Missing parameters." -ForegroundColor Red
    Write-Host "Usage: ./ef_migrate.ps1 <ProjectName> <ServiceName> [MigrationName]" -ForegroundColor Yellow
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

Write-Host "[>] Running EF Core migration for service '$ServiceName'..." -ForegroundColor Cyan

# ---------------- Add Migration ----------------
dotnet ef migrations add $MigrationName `
    --project "$InfraProject" `
    --startup-project "$ApiProject" `
    --output-dir "Persistence/Migrations"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Migration failed." -ForegroundColor Red
    exit 1
}

# ---------------- Update Database ----------------
dotnet ef database update `
    --project "$InfraProject" `
    --startup-project "$ApiProject"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Database update failed." -ForegroundColor Red
    exit 1
}

# ---------------- Scaffold DbContext (optional reverse engineering) ----------------
# Uncomment if you want to scaffold from an existing DB
# dotnet ef dbcontext scaffold "YourConnectionStringHere" Npgsql.EntityFrameworkCore.PostgreSQL `
#     --project "$InfraProject" `
#     --startup-project "$ApiProject" `
#     --output-dir "Persistence/Scaffold" `
#     --context "ProgramsDbContext" `
#     --force

Write-Host "`n[OK] Migration '$MigrationName' applied successfully for service '$ServiceName'." -ForegroundColor Green
