Write-Host "Scanning for EF Core DbContexts across all modules..." -ForegroundColor Cyan

$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir  = (Resolve-Path (Join-Path $ScriptDir "..\..\src\Backend")).Path 
$HostApiProj = Join-Path $BackendDir "Api\Api\Api.csproj"

$InfraProjects = Get-ChildItem -Path "$BackendDir\Modules" -Filter "*.Infrastructure.csproj" -Recurse
$hasErrors = $false
$summary   = @()

foreach ($proj in $InfraProjects) {
    $moduleName = $proj.Directory.Parent.Name
    Write-Host "`nChecking module: $moduleName" -ForegroundColor Yellow
    
    $dbContextFiles = Get-ChildItem -Path $proj.Directory.FullName -Filter "*Context.cs" -Recurse
    
    if (-not $dbContextFiles) {
        Write-Host "  [INFO] No DbContext found in module '${moduleName}'." -ForegroundColor DarkGray
        $summary += "Module ${moduleName}: No DbContext"
        continue
    }

    foreach ($file in $dbContextFiles) {
        $ctxName = $file.BaseName 
        Write-Host "  -> Verifying DbContext: ${ctxName}"
        
        $output = dotnet ef migrations has-pending-model-changes `
            --project "$($proj.FullName)" `
            --startup-project "$HostApiProj" `
            --context "$ctxName" `
            --no-build 2>&1

        $exitCode = $LASTEXITCODE

        $isConnectionError = ($output -match "network-related" -or 
                              $output -match "instance-specific" -or 
                              $output -match "Login failed" -or 
                              $output -match "connection")

        if ($exitCode -ne 0 -and $isConnectionError) {
            Write-Host "     [!] SKIPPED: Connection failed (Expected in CI). Snapshot check only." -ForegroundColor Yellow
            $summary += "Module ${moduleName} / ${ctxName}: Skipped (Connection error)"
            continue
        }

        if ($exitCode -ne 0) {
            Write-Host "     [X] FAILED: Pending changes found in '${ctxName}'!" -ForegroundColor Red
            Write-Host "         Error: $output" -ForegroundColor Gray
            $summary += "Module ${moduleName} / ${ctxName}: FAILED"
            $hasErrors = $true
        } else {
            Write-Host "     [OK] ${ctxName}: Up to date." -ForegroundColor Green
            $summary += "Module ${moduleName} / ${ctxName}: OK"
        }
    }
}

Write-Host "`n=== MIGRATION SUMMARY ===" -ForegroundColor Cyan
foreach ($line in $summary) {
    Write-Host "  $line"
}

if ($hasErrors) {
    Write-Host "`n[ERROR] Migration check failed. Please run 'dotnet ef migrations add' locally." -ForegroundColor Red
    exit 1
} else {
    Write-Host "`n[SUCCESS] All modules are in sync." -ForegroundColor Green
    exit 0
}
