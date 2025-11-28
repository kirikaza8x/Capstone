# === CAPSTONE SCRIPT LAUNCHER ===

# Map each option to its script path (all inside Scripts/Shells/)
$scriptMap = @{
    1 = "create_clean_arch.ps1"
    2 = "init_postgres.ps1"
    3 = "setup_docker_env.ps1"
    4 = "generate_ci_cd.ps1"
    5 = "add_base_di.ps1"
}

function Show-Menu {
    Write-Host ""
    Write-Host "=== CAPSTONE SCRIPT LAUNCHER ==="
    Write-Host "Choose an option:"
    Write-Host "1. Create Clean Architecture Microservice"
    Write-Host "2. Initialize PostgreSQL Database"
    Write-Host "3. Setup Docker Environment"
    Write-Host "4. Generate CI/CD Pipeline"
    Write-Host "5. Add Base DI + NuGet Packages"
    Write-Host "0. Exit"
    Write-Host ""
}

function Show-Guide {
    param([int]$Option)
    switch ($Option) {
        1 {
            Write-Host "`nGuide for Option 1:"
            Write-Host "Creates a microservice folder structure with Clean Architecture layers."
        }
        2 {
            Write-Host "`nGuide for Option 2:"
            Write-Host "Initializes PostgreSQL database with default schema and connection string."
        }
        3 {
            Write-Host "`nGuide for Option 3:"
            Write-Host "Sets up Docker environment with Dockerfile and docker-compose.yml."
        }
        4 {
            Write-Host "`nGuide for Option 4:"
            Write-Host "Generates CI/CD pipeline configuration (GitHub Actions, Azure DevOps, etc.)."
        }
        5 {
            Write-Host "`nGuide for Option 5:"
            Write-Host "Adds NuGet packages and scaffolds Dependency Injection setup for Domain, Application, Infrastructure, and Api layers."
        }
        default {
            Write-Host "`nNo guide available for this option yet."
        }
    }
}

function Run-Option {
    param([int]$Option)

    if ($Option -eq 0) {
        Write-Host "`nExiting. See you next time!"
        exit
    }

    if (-not $scriptMap.ContainsKey($Option)) {
        Write-Host "`nInvalid option. Please try again."
        return
    }

    Show-Guide -Option $Option

    $projectName = Read-Host "`nEnter Project Name (e.g., Capstone)"
    $serviceName = Read-Host "Enter Service Name (e.g., SmartCalo)"
    $solutionName = Read-Host "Enter Solution Name (default: $projectName.sln)"

    if ([string]::IsNullOrWhiteSpace($solutionName)) {
        $solutionName = "$projectName.sln"
    }

    $scriptName = $scriptMap[$Option]
    $scriptPath = Join-Path $PSScriptRoot $scriptName

    if (-not (Test-Path $scriptPath)) {
        Write-Host "`n[X] Script not found at: $scriptPath" -ForegroundColor Red
        return
    }

    & $scriptPath $projectName $serviceName $solutionName
}

do {
    Show-Menu
    $choice = Read-Host "Enter your choice"
    Run-Option -Option $choice
} while ($true)
# --- IGNORE ---