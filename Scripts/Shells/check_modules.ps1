param(
    [string]$ProjectName,
    [string]$ConnectionString
)

if (-not $ProjectName) {
    Write-Host "`n[X] Missing ProjectName." -ForegroundColor Red
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

$ModulesDir = Join-Path $BackendDir "Modules"
$modules = Get-ChildItem -Path $ModulesDir -Directory

foreach ($module in $modules) {
    $ModuleName = $module.Name
    $ApiProject   = Join-Path $module.FullName "$ModuleName.PublicApi\$ModuleName.PublicApi.csproj"
    $InfraProject = Join-Path $module.FullName "$ModuleName.Infrastructure\$ModuleName.Infrastructure.csproj"

    if (-not (Test-Path $ApiProject) -or -not (Test-Path $InfraProject)) {
        Write-Host "[!] Skipping $ModuleName (missing PublicApi or Infrastructure project)." -ForegroundColor Yellow
        continue
    }

    Write-Host "`n[>] Checking migrations for module '$ModuleName'..." -ForegroundColor Cyan

    dotnet ef migrations list `
        --project "$InfraProject" `
        --startup-project "$ApiProject" > "code_migrations_$ModuleName.txt"

    if (-not $ConnectionString) {
        Write-Host "[!] No ConnectionString provided. Skipping DB comparison for $ModuleName." -ForegroundColor Yellow
        continue
    }

    try {
        psql "$ConnectionString" -c "SELECT MigrationId FROM \"__EFMigrationsHistory\";" > "db_migrations_$ModuleName.txt"
        if (-not (Compare-Object (Get-Content "code_migrations_$ModuleName.txt") (Get-Content "db_migrations_$ModuleName.txt"))) {
            Write-Host "[OK] Database schema matches EF migrations for $ModuleName." -ForegroundColor Green
        } else {
            Write-Host "[X] Schema mismatch detected for $ModuleName!" -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "[!] Failed to query DB for $ModuleName." -ForegroundColor Yellow
    }
}
