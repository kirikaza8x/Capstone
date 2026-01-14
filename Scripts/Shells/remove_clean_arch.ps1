param(
    [string]$ProjectName,
    [string]$ModuleName
)

# Prompt if params are missing
if (-not $ProjectName) { $ProjectName = Read-Host "Enter ProjectName (e.g., Capstone)" }
if (-not $ModuleName) { $ModuleName = Read-Host "Enter ModuleName (e.g., Example)" }

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
    Write-Host "Tried:" -ForegroundColor Yellow
    Write-Host " - $BackendDirCandidateA" -ForegroundColor Yellow
    Write-Host " - $BackendDirCandidateB" -ForegroundColor Yellow
    exit 1
}

# Paths
$ModuleRoot   = Join-Path $BackendDir "Modules\$ModuleName"
$SolutionPath = Join-Path $BackendDir "$ProjectName.sln"

Write-Host "`n[!] Removing scaffold for module '$ModuleName'..." -ForegroundColor Cyan

# === Remove projects from solution ===
if (Test-Path $SolutionPath) {
    Write-Host "[+] Updating solution $SolutionPath" -ForegroundColor Yellow

    # Loop through all csproj files under the module and remove them
    $projFiles = Get-ChildItem -Path $ModuleRoot -Recurse -Filter *.csproj -ErrorAction SilentlyContinue
    foreach ($proj in $projFiles) {
        Write-Host "    Removing $($proj.FullName)" -ForegroundColor DarkGray
        dotnet sln "$SolutionPath" remove $proj.FullName 2>$null
    }
} else {
    Write-Host "[!] Solution file not found at $SolutionPath" -ForegroundColor Yellow
}

# === Delete module directory ===
if (Test-Path $ModuleRoot) {
    $confirm = Read-Host "Are you sure you want to delete module directory $ModuleRoot? (y/n)"
    if ($confirm -eq 'y') {
        # Preserve Dockerfile if present
        $dockerFile = Join-Path $ModuleRoot "Dockerfile"
        if (Test-Path $dockerFile) {
            $tempDocker = Join-Path $BackendDir "Dockerfile.$ModuleName.bak"
            Copy-Item $dockerFile $tempDocker -Force
            Write-Host "[+] Preserved Dockerfile as $tempDocker" -ForegroundColor Green
        }

        Remove-Item -Recurse -Force $ModuleRoot
        Write-Host "[+] Deleted module directory $ModuleRoot" -ForegroundColor Green

        # Restore Dockerfile if it was preserved
        if (Test-Path $tempDocker) {
            $restorePath = Join-Path $BackendDir "Modules\$ModuleName\Dockerfile"
            New-Item -ItemType Directory -Force -Path (Split-Path $restorePath -Parent) | Out-Null
            Move-Item $tempDocker $restorePath
            Write-Host "[+] Restored Dockerfile to $restorePath" -ForegroundColor Green
        }
    } else {
        Write-Host "[!] Skipped deleting module directory." -ForegroundColor Yellow
    }
} else {
    Write-Host "[!] Module directory not found at $ModuleRoot" -ForegroundColor Yellow
}

Write-Host "`n[OK] Removal complete for module '$ModuleName' (Dockerfile preserved)." -ForegroundColor Green
