param(
    [string]$MigrationName
)

$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = $null

# 1. Locate Backend Directory
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

# 2. Locate Startup Project
$HostApiProj = Join-Path $BackendDir "Api\Api\Api.csproj"
if (-not (Test-Path $HostApiProj)) {
    Write-Host "[X] Host API project not found at $HostApiProj." -ForegroundColor Red
    exit 1
}

# 3. Setup Migration Naming
$Timestamp = Get-Date -Format "yyyyMMddHHmmss"
$BaseName  = if ($MigrationName) { $MigrationName } else { "migrate_auto" }
$FullMigrationName = "${BaseName}_$Timestamp"

Write-Host "`nScanning for EF Core DbContexts across all modules..." -ForegroundColor Cyan
Write-Host "Target Migration: $FullMigrationName" -ForegroundColor DarkGray

$AllInfraProjects = Get-ChildItem -Path "$BackendDir\Modules" -Filter "*.Infrastructure.csproj" -Recurse
$summary   = @()
$hasErrors = $false

foreach ($proj in $AllInfraProjects) {
    $moduleName = $proj.Directory.Parent.Name
    Write-Host "`nModule: $moduleName" -ForegroundColor Yellow

    $ContextFiles = Get-ChildItem -Path $proj.Directory.FullName -Filter "*Context.cs" -Recurse |
                    Where-Object { $_.Name -notmatch "Factory" }

    if (-not $ContextFiles) {
        Write-Host "  [INFO] No DbContext found - skipping." -ForegroundColor DarkGray
        $summary += "$moduleName : No DbContext"
        continue
    }

    foreach ($file in $ContextFiles) {
        $ctxName = $file.BaseName
        Write-Host "  -> Verifying $ctxName..." -ForegroundColor White

        # --- FIX: CAPTURE AND USE OUTPUT ---
        $checkOutput = dotnet ef migrations has-pending-model-changes `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" `
            --no-build 2>&1
        
        $checkExitCode = $LASTEXITCODE

        if ($checkExitCode -eq 0) {
            Write-Host "    [OK] No changes detected." -ForegroundColor Gray
            $summary += "$moduleName / $ctxName : OK (No changes)"
            continue
        } 
        elseif ($checkExitCode -ne 1) {
            # THE VARIABLE IS NOW USED HERE:
            Write-Host "    [!] Error checking model for $ctxName. Output details:" -ForegroundColor Yellow
            Write-Host "    $($checkOutput | Out-String)" -ForegroundColor DarkGray
            
            $summary += "$moduleName / $ctxName : CHECK ERROR"
            $hasErrors = $true
            continue
        }

        # --- STEP 2: ADD MIGRATION ---
        Write-Host "    [>] Changes detected. Adding migration..." -ForegroundColor Cyan
        dotnet ef migrations add $FullMigrationName `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" `
            --output-dir "Persistence/Migrations"

        if ($LASTEXITCODE -ne 0) {
            Write-Host "    [X] Migration add failed." -ForegroundColor Red
            $summary += "$moduleName / $ctxName : FAILED (Add)"
            $hasErrors = $true
            continue
        }

        # --- STEP 3: UPDATE DATABASE ---
        Write-Host "    [>] Updating database..." -ForegroundColor Cyan
        $updateOutput = dotnet ef database update `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" 2>&1

        if ($LASTEXITCODE -ne 0) {
            if ($updateOutput -match "42P07" -or $updateOutput -match "already exists") {
                Write-Host "    [!] FAILED: Table already exists (42P07). Sync needed." -ForegroundColor Yellow
                $summary += "$moduleName / $ctxName : SYNC ERROR (Table Exists)"
            } else {
                Write-Host "    [X] Database update failed. Output:" -ForegroundColor Red
                Write-Host "    $($updateOutput | Out-String)" -ForegroundColor DarkGray
                $summary += "$moduleName / $ctxName : FAILED (Update)"
            }
            $hasErrors = $true
            continue
        }

        Write-Host "    [OK] $ctxName migrated successfully." -ForegroundColor Green
        $summary += "$moduleName / $ctxName : OK (Migrated)"
    }
}

Write-Host "`n================ MIGRATION SUMMARY ================" -ForegroundColor Cyan
foreach ($line in $summary) {
    if ($line -match "OK") { Write-Host "  $line" -ForegroundColor Green }
    elseif ($line -match "FAILED" -or $line -match "ERROR") { Write-Host "  $line" -ForegroundColor Red }
    else { Write-Host "  $line" -ForegroundColor Gray }
}

if ($hasErrors) {
    Write-Host "`n[X] Process completed with errors. See logs above." -ForegroundColor Red
    exit 1
}

Write-Host "`n[SUCCESS] All modules are processed." -ForegroundColor Green
exit 0