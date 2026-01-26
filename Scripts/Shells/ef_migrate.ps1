param(
    [string]$ProjectName,
    [string]$ModuleName,
    [string]$MigrationName,
    [string]$DbContextName
)

# Prompt if params are missing
if (-not $ProjectName)   { $ProjectName   = Read-Host "Enter ProjectName (e.g., Capstone)" }
if (-not $ModuleName)    { $ModuleName    = Read-Host "Enter ModuleName (e.g., Users)" }
if (-not $MigrationName) { $MigrationName = Read-Host "Enter MigrationName (default InitialCreate)" }
if (-not $DbContextName) { $DbContextName = Read-Host "Enter DbContext name (e.g., UserModuleDbContext)" }

# Default migration name if left blank
if (-not $MigrationName) { $MigrationName = "InitialCreate" }

# Resolve BackendDir relative to Scripts/Shells/run.ps1
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDirCandidateA = Join-Path $ScriptDir "..\..\..\src\Backend"
$BackendDirCandidateB = Join-Path $ScriptDir "..\..\src\Backend"

if (Test-Path $BackendDirCandidateA) {
    $BackendDir = (Resolve-Path $BackendDirCandidateA).Path
} elseif (Test-Path $BackendDirCandidateB) {
    $BackendDir = (Resolve-Path $BackendDirCandidateB).Path
} else {
    Write-Host "`n[X] Could not locate 'src\Backend' from $ScriptDir." -ForegroundColor Red
    exit 1
}

# Paths
$ModuleRoot   = Join-Path $BackendDir "Modules\$ModuleName"
$InfraProject = Join-Path $ModuleRoot "$ModuleName.Infrastructure\$ModuleName.Infrastructure.csproj"
$HostApiProj  = Join-Path $BackendDir "Api\Api\Api.csproj"   # global bootstrapper API

if (-not (Test-Path $InfraProject)) {
    Write-Host "`n[X] Infrastructure project not found for module '$ModuleName'." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $HostApiProj)) {
    Write-Host "`n[X] Host API project not found at $HostApiProj." -ForegroundColor Red
    exit 1
}

Write-Host "[>] Running EF Core migration for module '$ModuleName' using context '$DbContextName'..." -ForegroundColor Cyan

# ---------------- Add Migration ----------------
dotnet ef migrations add $MigrationName `
    --project "$InfraProject" `
    --startup-project "$HostApiProj" `
    --context "$DbContextName" `
    --output-dir "Persistence/Migrations"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Migration failed." -ForegroundColor Red
    exit 1
}

# ---------------- Update Database ----------------
dotnet ef database update `
    --project "$InfraProject" `
    --startup-project "$HostApiProj" `
    --context "$DbContextName"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[X] Database update failed." -ForegroundColor Red
    exit 1
}

Write-Host "`n[OK] Migration '$MigrationName' applied successfully for module '$ModuleName' with context '$DbContextName'." -ForegroundColor Green
exit 0
