param(
    [string]$SolutionPath,
    [switch]$CheckOnly,
    [switch]$NoRestore,
    [switch]$WhitespaceOnly,
    [switch]$WarnOnly
)

# Auto-detect solution path if not provided
if (-not $SolutionPath) {
    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $BackendDirCandidateA = Join-Path $ScriptDir "..\..\..\src\Backend"
    $BackendDirCandidateB = Join-Path $ScriptDir "..\..\src\Backend"

    if (Test-Path $BackendDirCandidateA) {
        $BackendDir = (Resolve-Path $BackendDirCandidateA).Path
    }
    elseif (Test-Path $BackendDirCandidateB) {
        $BackendDir = (Resolve-Path $BackendDirCandidateB).Path
    }
    else {
        Write-Host "ERROR: Could not locate src\Backend" -ForegroundColor Red
        exit 1
    }
    $SolutionPath = Join-Path $BackendDir "Capstone.sln"
}

# Verify solution exists
if (-not (Test-Path $SolutionPath)) {
    Write-Host "ERROR: Solution file not found: $SolutionPath" -ForegroundColor Red
    exit 1
}

Write-Host "Running dotnet format on: $SolutionPath" -ForegroundColor Cyan

if ($CheckOnly) {
    Write-Host "Mode: CHECK ONLY (will not modify files)" -ForegroundColor Yellow
}
if ($WarnOnly) {
    Write-Host "Mode: WARNING ONLY (will not fail on errors)" -ForegroundColor Yellow
}

# Build the format command
$formatArgs = @("format")

if ($WhitespaceOnly) {
    $formatArgs += "whitespace"
}

$formatArgs += $SolutionPath

if ($NoRestore) {
    $formatArgs += "--no-restore"
}

if ($CheckOnly) {
    $formatArgs += "--verify-no-changes"
}

$formatArgs += "--verbosity", "normal"

# Run dotnet format
Write-Host ""
Write-Host "Executing: dotnet $($formatArgs -join ' ')" -ForegroundColor Gray
dotnet @formatArgs
$exitCode = $LASTEXITCODE

# Handle result
if ($exitCode -ne 0) {
    Write-Host ""
    Write-Host "WARNING: Format check found issues" -ForegroundColor Yellow
    if (-not $WarnOnly) {
        Write-Host "To fix: run format.ps1 without -CheckOnly" -ForegroundColor Cyan
        exit 1
    }
    else {
        Write-Host "Continuing (warn-only mode)" -ForegroundColor Gray
        exit 0
    }
}

Write-Host ""
Write-Host "SUCCESS: Formatting check passed" -ForegroundColor Green
exit 0