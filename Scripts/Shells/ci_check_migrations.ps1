# ci_check_migrations.ps1
Write-Host "Scanning for EF Core DbContexts across all modules..." -ForegroundColor Cyan

# Resolve paths
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

$HostApiProj = Join-Path $BackendDir "Api\Api\Api.csproj"

# Find all Infrastructure projects in the Modules folder
$InfraProjects = Get-ChildItem -Path "$BackendDir\Modules" -Filter "*.Infrastructure.csproj" -Recurse

$hasErrors = $false

foreach ($proj in $InfraProjects) {
    $moduleName = $proj.Directory.Parent.Name
    Write-Host "`nChecking module: $moduleName" -ForegroundColor Yellow
    
    # SCAN THE FILES DIRECTLY: Look for anything ending in Context.cs inside the infrastructure folder
    $dbContextFiles = Get-ChildItem -Path $proj.Directory.FullName -Filter "*Context.cs" -Recurse
    
    if ($dbContextFiles.Count -eq 0) {
        Write-Host "  -> No DbContexts found for this module. Skipping." -ForegroundColor DarkGray
        continue
    }

    foreach ($file in $dbContextFiles) {
        # Get just the name without the .cs extension (e.g., "PaymentModuleDbContext")
        $ctxName = $file.BaseName 
        Write-Host "  -> Verifying DbContext: $ctxName"
        
        # Run the EF check. 
        # --no-build speeds it up since the CI pipeline already built the solution
        # 2>&1 and assigning to $null hides the noisy EF console output
        $null = dotnet ef migrations has-pending-model-changes `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" `
            --no-build 2>&1
            
        # $LASTEXITCODE will be 0 if up to date, or non-zero if changes are pending
        if ($LASTEXITCODE -ne 0) {
            Write-Host "     [X] FAILED: Pending changes found in '$ctxName'! You need to generate a migration." -ForegroundColor Red
            $hasErrors = $true
        } else {
            Write-Host "     [OK] Up to date." -ForegroundColor Green
        }
    }
}

if ($hasErrors) {
    Write-Host "`n[ERROR] One or more modules have pending EF Core changes. Pipeline failed." -ForegroundColor Red
    exit 1
} else {
    Write-Host "`n[SUCCESS] All database contexts across all modules are fully migrated." -ForegroundColor Green
    exit 0
}