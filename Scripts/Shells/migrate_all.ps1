param(
    [string]$MigrationName
)

$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = $null

foreach ($candidate in @("..\..\src\Backend", "..\..\..\src\Backend")) {
    $path = Join-Path $ScriptDir $candidate
    if (Test-Path $path) {
        $BackendDir = (Resolve-Path $path).Path
        break
    }
}

if (-not $BackendDir) {
    Write-Host "[X] Could not locate 'src\Backend' from $ScriptDir." -ForegroundColor Red
    exit 1
}

$HostApiProj = Join-Path $BackendDir "Api\Api\Api.csproj"
if (-not (Test-Path $HostApiProj)) {
    Write-Host "[X] Host API project not found at $HostApiProj." -ForegroundColor Red
    exit 1
}

$Timestamp     = Get-Date -Format "yyyyMMddHHmmss"
$MigrationName = if ($MigrationName) { "${MigrationName}_$Timestamp" } else { "migrate_auto_$Timestamp" }

Write-Host "Scanning for EF Core DbContexts across all modules..." -ForegroundColor Cyan
Write-Host "Migration name: $MigrationName`n" -ForegroundColor DarkGray

$AllInfraProjects = Get-ChildItem -Path "$BackendDir\Modules" -Filter "*.Infrastructure.csproj" -Recurse
$summary   = @()
$hasErrors = $false

foreach ($proj in $AllInfraProjects) {
    $moduleName = $proj.Directory.Parent.Name
    Write-Host "Module: $moduleName" -ForegroundColor Yellow

    $ContextFiles = Get-ChildItem -Path $proj.Directory.FullName -Filter "*Context.cs" -Recurse |
                    Where-Object { $_.Name -notmatch "Factory" }

    if (-not $ContextFiles) {
        Write-Host "  [INFO] No DbContext found — skipping." -ForegroundColor DarkGray
        $summary += "  $moduleName : Skipped (no DbContext)"
        continue
    }

    foreach ($file in $ContextFiles) {
        $ctxName = $file.BaseName
        Write-Host "  -> $ctxName" -ForegroundColor White

        # ── Add migration ──────────────────────────────────────────────────
        dotnet ef migrations add $MigrationName `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" `
            --output-dir "Persistence/Migrations" 2>&1 | Write-Host

        if ($LASTEXITCODE -ne 0) {
            Write-Host "     [X] Migration failed for $ctxName." -ForegroundColor Red
            $summary  += "  $moduleName / $ctxName : FAILED (migration)"
            $hasErrors  = $true
            continue
        }

        # ── Update database ────────────────────────────────────────────────
        dotnet ef database update `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" 2>&1 | Write-Host

        if ($LASTEXITCODE -ne 0) {
            Write-Host "     [X] Database update failed for $ctxName." -ForegroundColor Red
            $summary  += "  $moduleName / $ctxName : FAILED (db update)"
            $hasErrors  = $true
            continue
        }

        Write-Host "     [OK] $ctxName migrated." -ForegroundColor Green
        $summary += "  $moduleName / $ctxName : OK"
    }
}

Write-Host "`n=== MIGRATION SUMMARY ===" -ForegroundColor Cyan
foreach ($line in $summary) { Write-Host $line }

if ($hasErrors) {
    Write-Host "`n[X] Some migrations failed." -ForegroundColor Red
    exit 1
}

Write-Host "`n[OK] All modules migrated successfully with '$MigrationName'." -ForegroundColor Green
exit 0