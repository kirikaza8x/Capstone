param(
    [string]$ProjectName,
    [string]$ModuleName,
    [string]$ConnectionString,
    [string]$DbContextName = "ProgramsDbContext"
)

if (-not $ProjectName -or -not $ModuleName -or -not $ConnectionString) {
    Write-Host "`n[X] Missing parameters." -ForegroundColor Red
    Write-Host "Usage: ./ef_scaffold.ps1 <ProjectName> <ModuleName> <ConnectionString> [DbContextName]" -ForegroundColor Yellow
    exit 1
}

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

# Paths for module projects
$ModuleRoot   = Join-Path $BackendDir "Modules\$ModuleName"
$ApiProject   = Join-Path $ModuleRoot "$ModuleName.PublicApi\$ModuleName.PublicApi.csproj"
$InfraProject = Join-Path $ModuleRoot "$ModuleName.Infrastructure\$ModuleName.Infrastructure.csproj"

if (-not (Test-Path $ApiProject)) {
    Write-Host "`n[X] PublicApi project not found for module '$ModuleName'." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $InfraProject)) {
    Write-Host "`n[X] Infrastructure project not found for module '$ModuleName'." -ForegroundColor Red
    exit 1
}

Write-Host "[>] Scaffolding DbContext for module '$ModuleName'..." -ForegroundColor Cyan

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

Write-Host "`n[OK] DbContext '$DbContextName' scaffolded successfully for module '$ModuleName'." -ForegroundColor Green
