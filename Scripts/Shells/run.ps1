# === CAPSTONE SCRIPT LAUNCHER (Auto-Detect Files) ===

# Detect all .ps1 scripts in the same folder as run.ps1
$scriptFolder = $PSScriptRoot
$scriptFiles  = Get-ChildItem -Path $scriptFolder -Filter *.ps1

# Build dynamic map (skip the launcher itself)
$scriptMap = @{}
$index = 1
foreach ($file in $scriptFiles) {
    if ($file.Name -ne "run.ps1") {
        $scriptMap[$index] = $file.FullName
        $index++
    }
}

function Show-Menu {
    Write-Host ""
    Write-Host "=== CAPSTONE SCRIPT LAUNCHER ===" -ForegroundColor Cyan
    Write-Host "Choose a script to run:" -ForegroundColor Yellow

    foreach ($key in $scriptMap.Keys | Sort-Object) {
        $scriptName = [System.IO.Path]::GetFileName($scriptMap[$key])
        Write-Host "$key. $scriptName" -ForegroundColor Green
    }

    Write-Host "0. Exit" -ForegroundColor Red
    Write-Host ""
}

function RunOption {
    param([int]$Option)

    if ($Option -eq 0) {
        Write-Host "`nExiting. See you next time!" -ForegroundColor Magenta
        exit
    }

    if (-not $scriptMap.ContainsKey($Option)) {
        Write-Host "`n[X] Invalid option. Please try again." -ForegroundColor Red
        return
    }

    $scriptPath = $scriptMap[$Option]
    $scriptName = [System.IO.Path]::GetFileNameWithoutExtension($scriptPath)

    Write-Host "`n[+] Running $scriptName..." -ForegroundColor Cyan

    # Run the selected script directly
    & $scriptPath
}

do {
    Show-Menu
    $choice = Read-Host "Enter your choice"
    RunOption -Option $choice
} while ($true)
